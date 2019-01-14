using System;
using BEPUphysics.Entities;
using BEPUutilities;
using FixMath.NET;
using Lockstep.Framework.FastCollections;
using Lockstep.Framework.Networking.Messages;
using Lockstep.Framework.Pathfinding;

namespace Lockstep.Framework.Abilities
{
	public class Move : ILockstepAbility
    {
        Vector2 desiredVelocity;

        //Stop multipliers determine accuracy required for stopping on the destination
        public Fix64 FormationStop = Fix64.One / 4;
		public Fix64 GroupDirectStop = Fix64.One;
		public Fix64 DirectStop = Fix64.One / 4;

		private const int MinimumOtherStopTime = (int)(Simulation.FRAMERATE / 4);
		private const int StuckTimeThreshold = Simulation.FRAMERATE / 4;
		private const int StuckRepathTries = 4;

		public int GridSize { get { return (Agent.Radius).CeilToInt (); } }


		public Vector2 Position { get { return new Vector2(cachedBody.Position.X, cachedBody.Position.Y); } }
                                                                         
		public bool IsMoving { get; private set; }

		private bool hasPath;
		private bool straightPath;
		private bool viableDestination;
		private readonly FastList<Vector2> myPath = new FastList<Vector2> ();
		private int _pathIndex;

		private int pathIndex {
			get { return _pathIndex; }
			set {
				if (value != _pathIndex) {
					_pathIndex = value;
				}
			}
		}

		private int StoppedTime;
		private Vector2 targetPos;

		public Vector2 Destination;


		#region Auto stopping
		public bool GetCanAutoStop ()
		{
			return AutoStopPauser <= 0;
		}

		public bool GetCanCollisionStop ()
		{
			return CollisionStopPauser <= 0;
		}

		const int AUTO_STOP_PAUSE_TIME = Simulation.FRAMERATE / 8;
		private int AutoStopPauser;

		public void PauseAutoStop ()
		{
			AutoStopPauser = AUTO_STOP_PAUSE_TIME;
		}

		private int StopPauseLayer;

		private int CollisionStopPauser;

		public void PauseCollisionStop ()
		{
			CollisionStopPauser = AUTO_STOP_PAUSE_TIME;
		}

		//TODO: Improve the naming
		bool GetLookingForStopPause () {
			return StopPauseLooker >= 0;
		}

		private int StopPauseLooker;
		/// <summary>
		/// Start the search process for collisions/obstructions that are in the same attack group.
		/// </summary>
		public void StartLookingForStopPause () {
			StopPauseLooker = AUTO_STOP_PAUSE_TIME;
		}
		#endregion
		public Fix64 StopMultiplier { get; set; }

		//Has this unit arrived at destination? Default set to false.
		public bool Arrived { get; private set; }

		//Called when unit arrives at destination
		public event Action onArrive;

		public event Action onStartMove;

		//Called whenever movement is stopped... i.e. to attack
		public event Action OnStopMove;


		private Entity cachedBody { get; set; }
                                                     

		public Vector2 AveragePosition { get; set; }

		private Fix64 timescaledAcceleration;
		private Fix64 timescaledDecceleration;     

		private Vector2 lastTargetPos;
		private Vector2 targetDirection;

		private Vector2 waypointDirection { get { return this.targetDirection; } }

		private GridNode currentNode;
		private GridNode destinationNode;
		private Vector2 movementDirection;
		private Fix64 distance;
		private Fix64 closingDistance;
		private Fix64 stuckTolerance;
                            
		public bool SlowArrival { get; set; }

		#region Serialized                         
                                      

		public bool CanTurn { get; set; }

        public virtual Fix64 Speed { get; set;  } = Fix64.One;


        public Fix64 Acceleration { get; set; } = Fix64.One;

        private bool _canPathfind = true;

		public bool CanPathfind { get; private set; }
        public ILockstepAgent Agent;

        #endregion

        public void Setup (ILockstepAgent agent)
        {
            Agent = agent;
            cachedBody = agent.Body;
			//cachedBody.onContact += HandleCollision;    

			timescaledAcceleration = Fix64.SafeMul(Acceleration, Speed) / Simulation.FRAMERATE;
			//Cleaner stops with more decelleration
			timescaledDecceleration = timescaledAcceleration * 4;
			//Fatter objects can afford to land imprecisely
			closingDistance = agent.Radius;
			stuckTolerance = ((agent.Radius * Speed).RawValue >> 32) / Simulation.FRAMERATE;
			stuckTolerance *= stuckTolerance;
			CanPathfind = _canPathfind;
			this.SlowArrival = true;

            AveragePosition = Position;
        }

        public Move()
        {

            myPath.FastClear();
            _pathIndex = 0;
            StoppedTime = 0;      

            AutoStopPauser = 0;
            CollisionStopPauser = 0;
            StopPauseLooker = 0;
            StopPauseLayer = 0;

            StopMultiplier = DirectStop;

            viableDestination = false;

            Destination = Vector2.Zero;
            hasPath = false;
            IsMoving = false;
            StuckTime = 0;
            RepathTries = 0;

            Arrived = true;
            DoPathfind = false;

        }    

		private int StuckTime {
			get { return _stuckTime; }
			set {
				_stuckTime = value;
				if (_stuckTime == 0) {
				}
			}
		}

		private int _stuckTime;

		private int RepathTries;

		bool DoPathfind;


		uint GetNodeHash (GridNode node)
		{
			//TODO: At the moment, the CombinePathVersion is based on the destination... essentially caching the path to the last destination
			//Should this be based on commands instead?
			//Also, a lot of redundancy can be moved into MovementGroupHelper... i.e. getting destination node 
			uint ret = (uint)(node.gridX * GridManager.Width);
			ret += (uint)node.gridY;
			return ret;
		}

		public void Simulate ()
		{      
			//TODO: Organize/split this function
			if (IsMoving) {                       
				if (CanPathfind) {
					if (DoPathfind) {
						DoPathfind = false;
						if (viableDestination) {
							if (Pathfinder.GetStartNode (Position, out currentNode)) {
								if (currentNode.DoesEqual (this.destinationNode)) {
									if (this.RepathTries >= 1) {
										this.Arrive ();
									}
								} else {
									if (straightPath) {
										if (Pathfinder.NeedsPath (currentNode, destinationNode, this.GridSize)) {
											if (Pathfinder.FindPath (Destination, currentNode, destinationNode, myPath,
												    GridSize, GetNodeHash (destinationNode))) {
												hasPath = true;
												pathIndex = 0;
											} else {
												//if (IsFormationMoving) {
												//	StartMove (MyMovementGroup.Destination);
												//	IsFormationMoving = false;
												//}
											}
											straightPath = false;
										} else {
										}
									} else {
										if (Pathfinder.NeedsPath (currentNode, destinationNode, this.GridSize)) {
											if (Pathfinder.FindPath (Destination, currentNode, destinationNode, myPath,
												    GridSize, GetNodeHash (destinationNode))) {
												hasPath = true;
												pathIndex = 0;
											} else {
												//if (IsFormationMoving) {
												//	StartMove (MyMovementGroup.Destination);
												//	IsFormationMoving = false;
												//}
											}
										} else {
											straightPath = true;
										}
									}
								}
							} else {

							}
						} else {
							hasPath = false;
							//if (IsFormationMoving) {
							//	StartMove (MyMovementGroup.Destination);
							//	IsFormationMoving = false;
							//}
						}
					} else {

					}

					if (straightPath) {
						targetPos = Destination;
					} else if (hasPath) {
						if (pathIndex >= myPath.Count) {
							targetPos = this.Destination;
						} else {
							targetPos = myPath [pathIndex];
						}
					} else {
						targetPos = Destination;
					}
				} else {
					targetPos = Destination;
				}

				movementDirection = targetPos - Position;

				movementDirection.Normalize (out distance);
				if (targetPos.X != lastTargetPos.X || targetPos.Y != lastTargetPos.Y) {
					lastTargetPos = targetPos;
					targetDirection = movementDirection;
				}
				bool movingToWaypoint = (this.hasPath && this.pathIndex < myPath.Count - 1);
				Fix64 stuckThreshold = timescaledAcceleration / Simulation.FRAMERATE;

                Fix64 slowDistance = cachedBody.LinearVelocity.Length() / (timescaledDecceleration);


				if (distance > slowDistance || movingToWaypoint) {
					desiredVelocity = (movementDirection);
					//if (CanTurn)
					//	CachedTurn.StartTurnDirection (movementDirection);
				}
				else {

					if (distance < Fix64.SafeMul(closingDistance, StopMultiplier)) {
						Arrive ();
						//TODO: Don't skip this frame of slowing down
						return;
					}
					if (distance > closingDistance) {
						//if (CanTurn)
						//	CachedTurn.StartTurnDirection (movementDirection);
					}
					if (distance <= slowDistance) {
						Fix64 closingSpeed = distance / (slowDistance);
						//if (CanTurn)
						//	CachedTurn.StartTurnDirection (movementDirection);
						desiredVelocity = movementDirection * closingSpeed;     
						//Reduce occurence of units preventing other units from reaching destination
						stuckThreshold *= 4;
					}


				}
				//If unit has not moved stuckThreshold in a frame, it's stuck
				StuckTime++;
				if (GetCanAutoStop ()) {
						if (Position.FastDistance(AveragePosition) <= (stuckThreshold * stuckThreshold)) {

						if (StuckTime > StuckTimeThreshold) {
							if (movingToWaypoint) {
								this.pathIndex++;
							} else {
								if (RepathTries < StuckRepathTries) {
									DoPathfind = true;
									RepathTries++;
								} else {
									RepathTries = 0;
									this.Arrive ();
								}
							}
							StuckTime = 0;
						}
					} else {
						if (StuckTime > 0)
							StuckTime -= 1;

						RepathTries = 0;
					}
				}
				if (movingToWaypoint) {

					if (
						(
						    this.pathIndex >= 0 &&
						    distance < closingDistance &&
						    Vector2.Dot(movementDirection, waypointDirection) < 0
						) ||
						distance < Fix64.SafeMul(closingDistance, F64.C0p5)) {
						this.pathIndex++;
					}
				}

				desiredVelocity *= Speed;
				cachedBody.LinearVelocity += GetAdjustVector (new Vector3(desiredVelocity.X, desiredVelocity.Y, 0));   


			} else {                   
				//Slowin' down
				if (cachedBody.LinearVelocity.Length() > 0) {
					cachedBody.LinearVelocity += GetAdjustVector (Vector3.Zero);
				}
				StoppedTime++;
			}                        

			AutoStopPauser--;
			CollisionStopPauser--;
			StopPauseLooker--;
			AveragePosition = AveragePosition.Lerped (Position, Fix64.One / 2);
			
		}

		Vector3 GetAdjustVector (Vector3 desiredVel)
		{
			var adjust = desiredVel - cachedBody.LinearVelocity;
			var adjustFastMag = adjust.Length();    

			if (adjustFastMag > timescaledAcceleration * (timescaledAcceleration)) {
				var mag = Fix64.Sqrt (adjustFastMag);
				adjust *= timescaledAcceleration / (mag);
			}
			return adjust;
		}
                                     

		protected void OnExecute (Command com)
		{  
		}
                

		public void Arrive ()
		{
			StopMove ();
            

			//TODO: Reset this variables when changing destination/command
			AutoStopPauser = 0;
			CollisionStopPauser = 0;
			StopPauseLooker = 0;
			StopPauseLayer = 0;

			Arrived = true;
		}
              

		public void StopMove ()
		{
			if (IsMoving) {

				//if (MyMovementGroup.IsNotNull ()) {
				//	MyMovementGroup.Remove (this);
				//}

				IsMoving = false;
				StoppedTime = 0;
                                    
				if (OnStopMove.IsNotNull ()) {
					OnStopMove ();
				}
			}
		}

		public void OnGroupProcessed (Vector2 destination)
		{
			Destination = destination;
			if (MoveOnGroupProcessed) {
				StartMove (destination);
				MoveOnGroupProcessed = false;
			} else {
				this.Destination = destination;
			}
			if (this.onGroupProcessed != null)
				this.onGroupProcessed ();
		}

		public event Action onGroupProcessed;

		public bool MoveOnGroupProcessed { get; private set; }




		public void StartMove (Vector2 destination)
		{
			DoPathfind = true;
			hasPath = false;
			straightPath = false;
			this.Destination = destination;
			IsMoving = true;
			StoppedTime = 0;
			Arrived = false;

			//For now, use old next-best-node system when size requires consideration
			viableDestination = this.GridSize <= 1 ?
				Pathfinder.GetEndNode (Position, destination, out destinationNode) :
                Pathfinder.GetClosestViableNode (Position, destination, this.GridSize, out destinationNode);
			//TODO: If next-best-node, autostop more easily
			//Also implement stopping sooner based on distance

			StuckTime = 0;
			RepathTries = 0;
            onStartMove?.Invoke ();
        }       


		//private int collidedCount;
		//private ushort collidedID;

		//static LockstepAgent tempAgent;

		//bool paused;
		//private void HandleCollision (LSBody other)
		//{
  //          if (!CanMove)
  //          {
  //              return;
  //          }
  //          if ((tempAgent = other.Agent) == null)
  //          {
  //              return;
  //          }

  //          Move otherMover = tempAgent.GetAbility<Move>();
  //          if (ReferenceEquals(otherMover, null) == false)
  //          {
  //              if (IsMoving)
  //              {
  //                  //If the other mover is moving to a similar point
  //                  if (otherMover.MyMovementGroupID == MyMovementGroupID || otherMover.targetPos.FastDistance(this.targetPos) <= (closingDistance * closingDistance))
  //                  {
  //                      if (otherMover.IsMoving == false)
  //                      {
  //                          if (otherMover.Arrived && otherMover.StoppedTime > MinimumOtherStopTime)
  //                          {
  //                              Arrive();
  //                          }
  //                      }
  //                      else
  //                      {
  //                          if (hasPath && otherMover.hasPath && otherMover.pathIndex > 0 && otherMover.lastTargetPos.SqrDistance(targetPos.x, targetPos.y) < closingDistance.Mul(closingDistance))
  //                          {
  //                              if (this.distance < this.closingDistance)
  //                              {
  //                                  this.pathIndex++;
  //                              }
  //                          }
  //                      }
  //                  }

  //                  if (GetLookingForStopPause())
  //                  {
  //                      //As soon as the original collision stop unit is released, units will start breaking out of pauses
  //                      if (otherMover.GetCanCollisionStop() == false)
  //                      {
  //                          StopPauseLayer = -1;
  //                          PauseAutoStop();
  //                      }
  //                      else if (otherMover.GetCanAutoStop() == false)
  //                      {
  //                          if (otherMover.StopPauseLayer < StopPauseLayer)
  //                          {
  //                              StopPauseLayer = otherMover.StopPauseLayer + 1;
  //                              PauseAutoStop();
  //                          }
  //                      }
  //                  }
  //                  else
  //                  {

  //                  }
  //              }
  //          }
  //      } 
	}
}
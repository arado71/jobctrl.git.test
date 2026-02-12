using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;

namespace Tct.ActivityRecorderClient.Controller
{
	public abstract class CurrentWorkControllerBase : INotifyPropertyChanged
	{
		//private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public event PropertyChangedEventHandler PropertyChanged;

		private WorkState currentWorkState;
		public WorkState CurrentWorkState
		{
			get { return currentWorkState; }
			private set { UpdateField(ref currentWorkState, value, "CurrentWorkState"); }
		}

		private WorkData currentWork;
		public WorkData CurrentWork
		{
			get { return currentWork; }
			private set { UpdateField(ref currentWork, value, "CurrentWork"); }
		}

		protected WorkState LastWorkState { get; set; }
		public WorkStateChangeReason LastWorkStateChangeReason { get; protected set; }
		public virtual WorkData LastUserSelectedOrPermWork { get; protected set; }

		protected abstract void StopWork();
		protected abstract void StartWork(WorkData workData);
		protected virtual bool CanStartWork(ref WorkData workData, WorkState newWorkState, WorkStateChangeReason changeReason) { return true; }
		protected virtual void CannotStartWork(WorkData workData, WorkState newWorkState, WorkStateChangeReason changeReason) { }

		private void SetCurrentWorkImpl(WorkData workData, WorkState newWorkState, WorkStateChangeReason changeReason)
		{
			if (workData == null)
			{
				Debug.Assert(newWorkState == WorkState.NotWorking || newWorkState == WorkState.NotWorkingTemp);
				StopWork();
			}
			else
			{
				Debug.Assert(workData.Id.HasValue);
				Debug.Assert(newWorkState == WorkState.Working || newWorkState == WorkState.WorkingTemp);
				if (changeReason != WorkStateChangeReason.AutoResume && !CanStartWork(ref workData, newWorkState, changeReason))
				{
					CannotStartWork(workData, newWorkState, changeReason);
					return;
				}
				Debug.Assert(workData.Id.HasValue);
				Debug.Assert(MenuCoordinator.IsWorkIdFromServer(workData.Id.Value) || workData.AssignData != null);
				StartWork(workData);
				if (newWorkState == WorkState.Working)
				{
					LastUserSelectedOrPermWork = workData;
				}
			}
			var currentWorkStateTemp = currentWorkState;
			currentWorkState = newWorkState;
			currentWork = workData;
			LastWorkStateChangeReason = changeReason;
			//only raise when all fields are set
			LastWorkState = currentWorkStateTemp;
			OnPropertyChanged("CurrentWorkState");
			OnPropertyChanged("CurrentWork");
		}

		public virtual void UserStartWork(WorkData workData)
		{
			if (workData == null || !workData.Id.HasValue) return; //error invalid work
			if (CurrentWorkState == WorkState.Working && CurrentWork != null && workData.Id.Value == CurrentWork.Id.Value) return; //don't start same work
			SetCurrentWorkImpl(workData, WorkState.Working, WorkStateChangeReason.UserSelect);
		}

		public virtual void TempStartWork(WorkData workData, bool isEnabledWhileNotWorking = false)
		{
			if (workData == null || !workData.Id.HasValue) return; //error invalid work
			if (CurrentWorkState == WorkState.NotWorking && !isEnabledWhileNotWorking) return; //TempStartWork cannot start work when not working
			if (CurrentWork != null && workData.Id.Value == CurrentWork.Id.Value) return; //don't start same autodetected work
			SetCurrentWorkImpl(workData, WorkState.WorkingTemp, WorkStateChangeReason.AutodetectedTemp);
		}

		public virtual void PermStartWork(WorkData workData, bool isEnabledWhileNotWorking = false)
		{
			if (workData == null || !workData.Id.HasValue) return; //error invalid work
			if (CurrentWorkState == WorkState.NotWorking && !isEnabledWhileNotWorking) return; //PermStartWork cannot start work when not working
			if (CurrentWorkState == WorkState.Working && CurrentWork != null && workData.Id.Value == CurrentWork.Id.Value) return; //don't start same autodetected work
			SetCurrentWorkImpl(workData, WorkState.Working, WorkStateChangeReason.AutodetectedPerm);
		}

		public virtual void UserStopWork()
		{
			if (CurrentWorkState == WorkState.NotWorking) return; //NotWorking anyway
			SetCurrentWorkImpl(null, WorkState.NotWorking, WorkStateChangeReason.UserSelect);
		}

		public virtual void TempStopWork()
		{
			if (CurrentWorkState == WorkState.NotWorking) return; //NotWorking state cannot be overwritten with NotWorkingTemp state
			if (CurrentWorkState == WorkState.NotWorkingTemp) return; //don't stop autodetected work multiple times
			SetCurrentWorkImpl(null, WorkState.NotWorkingTemp, WorkStateChangeReason.AutodetectedTemp);
		}

		public virtual void UserResumeWork(WorkStateChangeReason wscr = WorkStateChangeReason.UserResume)
		{
			if (CurrentWorkState != WorkState.NotWorking) return;
			if (LastUserSelectedOrPermWork == null) //cannot resume
			{
				CannotStartWork(null, WorkState.Working, WorkStateChangeReason.UserResume);
			}
			else
			{
				SetCurrentWorkImpl(LastUserSelectedOrPermWork, WorkState.Working, wscr);
			}
		}

		public virtual void TempEndEffect()
		{
			if (CurrentWorkState != WorkState.NotWorkingTemp && CurrentWorkState != WorkState.WorkingTemp) return; //no temp effect to stop
			if (LastUserSelectedOrPermWork == null) //cannot switch back or start work
			{
				CannotStartWork(null, WorkState.Working, WorkStateChangeReason.AutodetectedEndTempEffect);
			}
			else
			{
				SetCurrentWorkImpl(LastUserSelectedOrPermWork, WorkState.Working, WorkStateChangeReason.AutodetectedEndTempEffect);
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null) propChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool UpdateField<T>(ref T field, T value, string propertyName)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}
	}
}
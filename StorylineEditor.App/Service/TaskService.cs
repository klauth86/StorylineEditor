/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System;
using System.Threading;

namespace StorylineEditor.App.Service
{
    public struct TaskFrame
    {
        public double StartTimeMsecMsec;

        public double DurationMsec;

        public Func<double, double, double, double, TaskStatusCustom> TickAction;

        public Action<TaskStatusCustom, double, double, double, double> FinAction;

        public Action<TaskStatusCustom> CallbackAction;

        public TaskStatusCustom TaskStatus;

        public bool IsDone;
    }

    public class TaskService : ITaskService
    {
        private static TaskFrame[] _taskFrames = { new TaskFrame(), new TaskFrame() };

        private static int _taskFrameIndex = 1;

        private static Thread _workerThread = null;

        private static double _timeMsec;

        private static double _prevTimeMsec;

        private static bool _isPaused = false;

        public void Stop()
        {
            var taskFrame = _taskFrames[_taskFrameIndex];
            taskFrame.IsDone = true;
        }

        public void SetIsPaused(bool isPaused) { _isPaused = isPaused; }

        public void Start(
            double durationMsec
            , Func<double, double, double, double, TaskStatusCustom> tickAction
            , Action<TaskStatusCustom, double, double, double, double> finAction
            , Action<TaskStatusCustom> callbackAction
            )
        {
            int nextTaskFrameIndex = 1 - _taskFrameIndex;

            _taskFrames[nextTaskFrameIndex].StartTimeMsecMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
            _taskFrames[nextTaskFrameIndex].DurationMsec = durationMsec;
            _taskFrames[nextTaskFrameIndex].TickAction = tickAction;
            _taskFrames[nextTaskFrameIndex].FinAction = finAction;
            _taskFrames[nextTaskFrameIndex].CallbackAction = callbackAction;
            _taskFrames[nextTaskFrameIndex].TaskStatus = TaskStatusCustom.WaitingToRun;
            _taskFrames[nextTaskFrameIndex].IsDone = false;

            Interlocked.Exchange(ref _taskFrameIndex, nextTaskFrameIndex);

            if (_workerThread == null)
            {
                CreateWorkerThread();
            }
        }

        private void CreateWorkerThread()
        {
            _workerThread = new Thread(() =>
            {
                while (true)
                {
                    _timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;

                    var taskFrame = _taskFrames[_taskFrameIndex];

                    TickTaskFrame(taskFrame);

                    _prevTimeMsec = _timeMsec;

                    Thread.Sleep(2);
                }
            });

            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        private void TickTaskFrame(TaskFrame taskFrame)
        {
            if (taskFrame.IsDone) return;

            double startTimeMsec = taskFrame.StartTimeMsecMsec;
            double durationMsec = taskFrame.DurationMsec;
            var tickAction = taskFrame.TickAction;
            var finAction = taskFrame.FinAction;
            var callbackAction = taskFrame.CallbackAction;

            // Init

            if (taskFrame.TaskStatus == TaskStatusCustom.WaitingToRun)
            {
                if (_timeMsec < startTimeMsec + durationMsec)
                {
                    taskFrame.TaskStatus = TaskStatusCustom.Running;
                }
                else
                {
                    taskFrame.TaskStatus = TaskStatusCustom.Timeout;
                }
            }

            if (taskFrame.TaskStatus != TaskStatusCustom.Running)
            {
                finAction(taskFrame.TaskStatus, startTimeMsec, durationMsec, _timeMsec, _timeMsec - _prevTimeMsec);

                callbackAction?.Invoke(taskFrame.TaskStatus);

                taskFrame.IsDone = true;
            }
            else
            {
                if (_timeMsec < startTimeMsec + durationMsec)
                {
                    taskFrame.TaskStatus = tickAction(startTimeMsec, durationMsec, _timeMsec, _timeMsec - _prevTimeMsec);
                }
                else
                {
                    finAction(taskFrame.TaskStatus, startTimeMsec, durationMsec, _timeMsec, _timeMsec - _prevTimeMsec);

                    callbackAction?.Invoke(taskFrame.TaskStatus);

                    taskFrame.IsDone = true;
                }
            }
        }
    }
}
/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorylineEditor.App.Service
{
    public class TaskService : ITaskService
    {
        private static long _lockIndex = 0;

        private static bool _isCancellationRequested;

        public bool IsPaused {get;set;}

        public async void Start(double durationMsec, Func<double, double, double, double, CustomStatus> tickAction, Func<double, double, double, double, CustomStatus, CustomStatus> finAction, Action<CustomStatus> callbackAction)
        {
            Stop();

            Interlocked.Add(ref _lockIndex, 1);

            while (Interlocked.Read(ref _lockIndex) > 1) await Task.Delay(2);

            _isCancellationRequested = false;

            CustomStatus customStatus = CustomStatus.WaitingToRun;

            try
            {
                double startTimeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                double finishTimeMsec = durationMsec >= 0 ? (startTimeMsec + durationMsec) : double.MaxValue;

                double prevTimeMsec = startTimeMsec;
                double timeMsec = startTimeMsec;
                double deltaTimeMsec = 0;

                while (timeMsec < finishTimeMsec)
                {
                    if (_isCancellationRequested)
                    {
                        customStatus = CustomStatus.Canceled;
                    }
                    else if (!IsPaused)
                    {
                        customStatus = tickAction(startTimeMsec, durationMsec, timeMsec, deltaTimeMsec);
                    }

                    if (customStatus == CustomStatus.RanToCompletion || customStatus == CustomStatus.Canceled || customStatus == CustomStatus.Faulted)
                    {
                        break;
                    }

                    await Task.Delay(2);

                    prevTimeMsec = timeMsec;
                    timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                    deltaTimeMsec = timeMsec - prevTimeMsec;
                }

                // Mark as completed if cycle was broken by time limits

                if (customStatus == CustomStatus.Running)
                {
                    customStatus = CustomStatus.RanToCompletion;
                }

                prevTimeMsec = timeMsec;
                timeMsec = DateTime.Now.TimeOfDay.TotalMilliseconds;
                deltaTimeMsec = timeMsec - prevTimeMsec;

                customStatus = finAction(startTimeMsec, durationMsec, timeMsec, deltaTimeMsec, customStatus);
            }
            catch (TaskCanceledException taskCanceledExc) { throw taskCanceledExc; }
            catch (Exception exc) { throw exc; }
            finally
            {
                Interlocked.Decrement(ref _lockIndex);

                callbackAction?.Invoke(customStatus);
            }
        }

        public void Stop()
        {
            _isCancellationRequested = true;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

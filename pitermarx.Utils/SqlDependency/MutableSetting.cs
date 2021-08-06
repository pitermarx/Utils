using System;

namespace pitermarx.SqlDependency
{
    public interface IMutableSetting : IDisposable
    {
        void RefreshValue(bool forceRefresh = false);
        bool IsDisposed { get; }
    }

    /// <summary>
    /// This is a class that holds a value.
    /// This value is always up to date because of the ProjectSettingsManager Connection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MutableSetting<T> : IMutableSetting
    {
        private readonly Func<T> getLatestSetting;

        public MutableSetting(Func<T> getter)
        {
            getLatestSetting = getter;
            RefreshValue(true);
        }

        /// <summary>
        /// The value. Cannot be set
        /// </summary>
        public virtual T Value { get; private set; }

        /// <summary>
        /// ProjectSettingsManager calls this when the value changes
        /// </summary>
        public void RefreshValue(bool forceRefresh = false)
        {
            var newValue = getLatestSetting();

            if (newValue == null)
            {
                return;
            }

            if (forceRefresh || Value == null || newValue.ToString() != Value.ToString())
            {
                OnChange?.Invoke(newValue);
            }

            // value only changes after event!
            // its on purpose!
            Value = newValue;
        }

        public bool IsDisposed { get; private set; }

        // subscribe to this event to get notified when the value changes
        public event Action<T> OnChange;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            IsDisposed = true;
            OnChange = null;
        }
    }
}
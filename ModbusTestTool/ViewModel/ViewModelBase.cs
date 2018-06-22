using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanging" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// TODO Edit XML Comment Template for ViewModelBase
    public abstract class ViewModelBase : INotifyPropertyChanging, INotifyPropertyChanged
    {
        #region INotifyPropertyChanging Members

        /// <summary>Occurs when a property value is changing.</summary>
        /// TODO Edit XML Comment Template for PropertyChanging
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>Occurs when a property value changes.</summary>
        /// TODO Edit XML Comment Template for PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Administrative Properties

        /// <summary>Whether the view model should ignore property-change events.</summary>
        /// <value>
        /// <c>true</c> if [ignore property change events]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IgnorePropertyChangeEvents { get; set; }

        #endregion

        #region Public Methods

        /// <summary>Raises the <see cref="E:PropertyChanged" /> event.</summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// TODO Edit XML Comment Template for OnPropertyChanged
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Raises the PropertyChanged event.</summary>
        /// <param name="propertyName">The name of the changed property.</param>
        public virtual void RaisePropertyChangedEvent(string propertyName)
        {
            // Exit if changes ignored
            if (IgnorePropertyChangeEvents) return;

            // Exit if no subscribers
            if (PropertyChanged == null) return;

            // Raise event
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged(this, e);
        }

        /// <summary>Raises the PropertyChanging event.</summary>
        /// <param name="propertyName">The name of the changing property.</param>
        public virtual void RaisePropertyChangingEvent(string propertyName)
        {
            // Exit if changes ignored
            if (IgnorePropertyChangeEvents) return;

            // Exit if no subscribers
            if (PropertyChanging == null) return;

            // Raise event
            var e = new PropertyChangingEventArgs(propertyName);
            PropertyChanging(this, e);
        }

        /// <summary>Raises the property changed.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// TODO Edit XML Comment Template for RaisePropertyChanged
        protected void RaisePropertyChanged(String propertyName)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Warns the developer if this Object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(String propertyName)
        {
            // verify that the property name matches a real,  
            // public, instance property on this Object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }

    }
}

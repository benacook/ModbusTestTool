using System;
using System.Windows.Input;

namespace Commands
{
    /// <summary>
    /// ///
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand"/>
    /// TODO Edit XML Comment Template for CommandBase
    public class CommandBase : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        /// <param name="executed">The executed.</param>
        /// TODO Edit XML Comment Template for #ctor
        public CommandBase(EventHandler executed)
        {
            IsExecutionPossible = true;
            Executed = executed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        public CommandBase()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// Private member of the 'IsExecutionPossible' property
        /// </summary>
        private bool _myIsExecutionPossible;

        /// <summary>
        /// Get or set the IsExecutionPossible
        /// </summary>
        /// <value><c>true</c> if this instance is execution possible; otherwise, <c>false</c>.</value>
        public bool IsExecutionPossible
        {
            get
            {
                return _myIsExecutionPossible;
            }

            set
            {
                if (_myIsExecutionPossible == value)
                {
                    return;
                }

                _myIsExecutionPossible = value;

                if (null != IsExecutionPossibleChanged)
                {
                    IsExecutionPossibleChanged(this, new EventArgs());
                }

                if (null != CanExecuteChanged)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Implementation of the interface members.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _myIsExecutionPossible;
        }

        /// <summary>
        /// Implementation of the interface members.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to null.
        /// </param>
        public virtual void Execute(object parameter)
        {
            if (null != Executed)
            {
                Executed(this, new EventArgs());
            }
        }

        /// <summary>
        /// Implementation of the interface members.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Implementation of the interface members.
        /// </summary>
        public event EventHandler Executed;

        /// <summary>
        /// Public event of the 'IsExecutionPossible' property, which signalizes that the property
        /// has changed.
        /// </summary>
        public event EventHandler IsExecutionPossibleChanged;
    }
}
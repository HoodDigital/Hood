using Hood.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hood.Infrastructure
{
    public class OperationResult<TObject> : OperationResult
    {
        public TObject Item { get; set; }

        public OperationResult(TObject item)
        {
            this.Succeeded = true;
            this.Item = item;
        }
    }

    public class OperationResult
    {
        public Exception Exception { get; set; }

        public AlertType Level { get; set; }

        /// <summary>
        /// Indicates whether the operation succeeded or not.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Indicates whether the operation succeeded or not.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A list of errors attached to the OperationResult.
        /// </summary>
        public IList<OperationError> Errors { get; set; }

        public string ErrorString
        {
            get
            {
                StringWriter sw = new StringWriter();
                foreach (OperationError error in Errors)
                {
                    sw.WriteLine(error.Description);
                }
                return sw.ToString();
            }
        }

        public OperationResult()
        {
            Succeeded = false;
            Errors = new List<OperationError>();
        }

        public OperationResult(AlertType level, string message)
        {
            Succeeded = true;
            Level = level;
            Message = message;
        }

        public OperationResult(string error)
        {
            Succeeded = false;
            Errors = new List<OperationError>();
            Errors.Add(new OperationError(error));
        }

        public OperationResult(IList<OperationError> errors)
        {
            Succeeded = false;
            Errors = errors;
        }

        public OperationResult(bool success)
        {
            Succeeded = success;
            Errors = new List<OperationError>();
        }

        public OperationResult(bool success, string error)
        {
            Succeeded = success;
            Errors = new List<OperationError>();
            Errors.Add(new OperationError(error));
        }

        public OperationResult(Exception exception)
        {
            this.Exception = exception;
            Succeeded = false;
            Errors = new List<OperationError>();
            this.Errors.Add(new OperationError(exception.Message));
        }

        public void AddError(string error)
        {
            Succeeded = false;
            Errors.Add(new OperationError(error));
        }

        public void ClearErrors()
        {
            Succeeded = true;
            Errors.Clear();
        }
    }

    public class OperationError
    {
        /// <summary>
        /// Sets up a blank OperationError.
        /// </summary>
        public OperationError()
        { }
        /// <summary>
        /// Sets up the OperationError with the given parameters.
        /// </summary>
        /// <param name="description">A description of the error.</param>
        public OperationError(string description)
        {
            Description = description;
        }
        /// <summary>
        /// Sets up the OperationError with the given parameters.
        /// </summary>
        /// <param name="code">The Code to represent the error.</param>
        /// <param name="description">A description of the error.</param>
        public OperationError(string code, string description)
        {
            Code = code;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the code for this error.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the description for this error.
        /// </summary>
        public string Description { get; set; }
    }
}

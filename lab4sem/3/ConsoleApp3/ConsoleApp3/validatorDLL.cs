using System;
using System.Collections.Generic;
using System.Linq;


namespace ValidatorDLL
{

    public interface IValidationRule<T>
    {
        void Validate(T data);
    }


    public class DataValidatorBuilder<T>
    {

        private readonly List<IValidationRule<T>> _rules = new List<IValidationRule<T>>();


        public DataValidatorBuilder<T> AddRule(IValidationRule<T> rule)
        {
            _rules.Add(rule);
            return this;
        }


        public UniversalValidator<T> Build()
        {
            if (_rules.Count == 0) { throw new NoValidationRulesException("No validation rules have been added.");  }

            return new UniversalValidator<T>(_rules);
        }
    }



    public class UniversalValidator<T>
    {
        private readonly List<IValidationRule<T>> _rules;


        internal UniversalValidator(List<IValidationRule<T>> rules)
        {
            _rules = rules;
        }

        // обработка правил валидатором

        public void Validate(T data)
        {
            var exceptions = new List<Exception>();

            foreach (var rule in _rules)
            {

                try
                {
                    rule.Validate(data);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

            }

            if ( exceptions.Count > 0 ){ throw new AggregateException(exceptions); }

        }

    }

    public class NoValidationRulesException : Exception
    {
        public NoValidationRulesException(string message) : base(message) {}
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message){}
    }

}
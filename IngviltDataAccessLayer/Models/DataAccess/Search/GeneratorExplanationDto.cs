namespace Ingvilt.Models.DataAccess.Search {
    public class GeneratorExplanationDto {
        public string Name {
            get;
        }

        public string ExampleValue {
            get;
            private set;
        }

        public string Description {
            get;
        }

        public ISearchQueryGenerator Generator {
            get;
        }

        public string ExampleUsageText {
            get {
                return $"{Name}:{ExampleValue}";
            }
        }

        private void SetExampleValue() {
            if (Generator is IGeneratorWithSpecificExampleValue genWithExample) {
                ExampleValue = genWithExample.GetExampleValue();
            } else if (Generator is QueryGeneratorWithIntValue) {
                ExampleValue = "3";
            } else if (Generator is QueryGeneratorWithArrayValue) {
                ExampleValue = "[\"value 1\", \"value 2\", \"value 3\"]";
            } else {
                ExampleValue = "value";
            }

            ExampleValue = $"'{ExampleValue.Replace("'", "\\'")}'";
        }

        public GeneratorExplanationDto(ISearchQueryGenerator generator) {
            Generator = generator;
            Name = generator.GetName();
            Description = generator.GetDescription();
            SetExampleValue();
        }
    }
}

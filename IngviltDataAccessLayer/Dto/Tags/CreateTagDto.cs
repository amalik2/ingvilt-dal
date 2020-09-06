namespace Ingvilt.Dto.Tags {
    public class CreateTagDto {
        public string Name {
            get;
        }

        public string Type {
            get;
        }

        public CreateTagDto(string name, string type) {
            Name = name;
            Type = type;
        }
    }
}

namespace Ingvilt.Dto.Tags {
    public class CreateVideoTagDto : CreateTagDto {
        public CreateVideoTagDto(string name, string type) : base(name, type) {
        }
    }
}

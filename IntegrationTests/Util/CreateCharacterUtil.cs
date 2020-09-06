using Ingvilt.Dto.Characters;

using System.Collections.Generic;

namespace IntegrationTestingRedo.Util {
    public class CreateCharacterUtil {
        public static CreateCharacterDto GetNewCharacterDetails(long libraryId) {
            return new CreateCharacterDto("Test character", "test desc", null, null, null, -1, libraryId, -1, -1);
        }
    }
}

using Ingvilt.Dto;
using Ingvilt.Dto.Characters;
using Ingvilt.Dto.Videos;

using System;
using System.Collections.Generic;

namespace IntegrationTesting.Util {
    public class CreateVideoUtil {
        public static CreateVideoDto GetNewVideoDetails(long libraryId, long publisherId, long seriesId) {
            return new CreateVideoDto(
                "test", 2, 3, 4, "desc", "notes", "C:/test.jpg", "C:/site.html",
                seriesId, VideoWatchStatus.UNDECIDED, publisherId, libraryId, new DateTime(), new DateTime(), -1,
                new List<ActorForCharacterFullDto>()
            );
        }
    }
}

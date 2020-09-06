using Ingvilt.Core;
using Ingvilt.Dto;
using Ingvilt.Dto.SeriesNS;
using Ingvilt.Dto.Tags;
using Ingvilt.Dto.Videos;
using Ingvilt.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ingvilt.Services {
    public class VideoTagService {
        private TagRepository tagRepository;

        public VideoTagService() {
            tagRepository = DependencyInjectionContainer.Container.Resolve<TagRepository>();
        }

        public async Task<List<Tag>> GetVideoTags() {
            return tagRepository.GetVideoTags();
        }

        public async Task<List<Tag>> GetDeletedVideoTags() {
            return tagRepository.GetDeletedVideoTags();
        }

        public VideoTag GetVideoTag(long tagId) {
            return tagRepository.GetVideoTag(tagId);
        }

        public long CreateVideoTag(CreateVideoTagDto dto) {
            return tagRepository.CreateVideoTag(dto);
        }

        public VideoTag CreateAndRetrieveVideoTag(CreateVideoTagDto dto) {
            long tagId = CreateVideoTag(dto);
            return new VideoTag(tagId, dto);
        }

        public async Task DeleteVideoTag(VideoTag tag) {
            tagRepository.DeleteVideoTag(tag);
        }

        public void UpdateVideoTag(VideoTag tag) {
            tagRepository.UpdateTag(tag);
        }

        public async Task RestoreVideoTag(VideoTag tag) {
            tagRepository.RestoreVideoTag(tag);
        }

        public async Task PermanentlyRemoveVideoTag(VideoTag tag) {
            tagRepository.PermanentlyRemoveVideoTag(tag);
        }

        public async Task<List<string>> LoadAllVideoTagNames() {
            return tagRepository.LoadAllVideoTagNames();
        }

        public async Task<List<Tag>> GetTagsOnVideo(Video video) {
            return tagRepository.GetTagsOnVideo(video.VideoId);
        }

        public async Task<List<Tag>> GetTagsOnMediaFile(MediaFile file) {
            return tagRepository.GetTagsOnMediaFile(file.MediaId);
        }

        public async Task<List<Tag>> GetTagsOnSeries(Series series) {
            return tagRepository.GetTagsOnSeries(series.SeriesId);
        }

        public void RemoveTagFromVideo(long videoId, long tagId) {
            tagRepository.RemoveTagFromVideo(videoId, tagId);
        }

        public void AddTagsToVideo(long videoId, List<long> tagIds) {
            tagRepository.AddTagsToVideo(videoId, tagIds);
        }

        public void RemoveTagFromMediaFile(long fileId, long tagId) {
            tagRepository.RemoveTagFromMediaFile(fileId, tagId);
        }

        public void AddTagsToMediaFile(long fileId, List<long> tagIds) {
            tagRepository.AddTagsToMediaFile(fileId, tagIds);
        }

        public void RemoveTagFromSeries(long seriesId, long tagId) {
            tagRepository.RemoveTagFromSeries(seriesId, tagId);
        }

        public void AddTagsToSeries(long seriesId, List<long> tagIds) {
            tagRepository.AddTagsToSeries(seriesId, tagIds);
        }
    }
}

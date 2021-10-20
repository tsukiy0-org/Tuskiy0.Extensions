using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsukiy0.Extensions.Data.Models;

namespace Tsukiy0.Extensions.Data.Services
{
    public abstract class AbstractVersionDaoMapper<T, U> : IDaoMapper<T, U>
    {
        protected readonly IList<VersionMapper<T, U>> versionMappers;

        public AbstractVersionDaoMapper(IList<VersionMapper<T, U>> versionMappers)
        {
            this.versionMappers = versionMappers;
        }

        public async Task<T> From(U u)
        {
            var dao = ToDao(u);
            var latestVersion = GetLatestVersionMapper().Version;
            var versionMapper = versionMappers.FirstOrDefault(_ => _.Version == dao.__VERSION);

            if (versionMapper is null)
            {
                throw new VersionMapperNotFoundException();
            }

            var dto = await versionMapper.Mapper.From(u);

            if (dao.__VERSION != latestVersion)
            {
                await OnVersionMismatch(dto, dao.__VERSION);
            }

            return dto;
        }

        public async Task<U> To(T t)
        {
            var mapper = GetLatestVersionMapper().Mapper;
            return await mapper.To(t);
        }

        protected abstract IDao ToDao(U u);

        protected abstract Task OnVersionMismatch(T dto, int currentVersion);

        private VersionMapper<T, U> GetLatestVersionMapper()
        {
            return versionMappers.Last();
        }
    }

    public record VersionMapper<T, U>(int Version, IDaoMapper<T, U> Mapper);

    public class VersionMapperNotFoundException : Exception { }
}

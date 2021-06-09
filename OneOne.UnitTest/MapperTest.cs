using AutoMapper;
using NUnit.Framework;
using OneOne.Utility4Core.ExtensionsMethod;
using System;

namespace OneOne.UnitTest
{
    /// <summary>
    /// .NET Core运行单元测试需要安装
    /// Microsoft.NET.Test.Sdk
    /// Nunit
    /// NUnit3TestAdapter
    /// </summary>
    [TestFixture]
    public class MapperTest
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            MapperExtension.CreateMapperByAssembly(new string[] { "OneOne.UnitTest" });
        }

        [Test]
        public void MapTo_When_Mapped_Return_Must_Not_Be_Bull()
        {
            var entity = new TrackRawDataDto { Product = ProductEnum.ERP, CreateTime = DateTime.Now };
            var result = MapperExtension.MapTo<TrackRawDataEntity, TrackRawDataDto>(entity);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ProductId, (sbyte)entity.Product);
            Assert.AreEqual(result.CreateTime, entity.CreateTime.Date);
        }

        /// <summary>
        /// 当转换关系未设置的时候抛出AutoMapperMappingException异常
        /// </summary>
        [Test]
        public void MapTo_When_Not_Mapper_Return_Must_ThrowExceprion()
        {
            var entity = new TrackRawDataEntity { ProductId = (sbyte)ProductEnum.ERP, CreateTime = DateTime.Now };
            Assert.Throws<AutoMapperMappingException>(()=> { MapperExtension.MapTo<TrackRawDataDto, TrackRawDataEntity>(entity); },message: " Missing type map configuration or unsupported mapping.");
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TrackRawDataDto, TrackRawDataEntity>()
                .ForMember(d => d.ProductId, p => p.MapFrom(x => (sbyte)x.Product))
                .ForMember(d => d.CreateTime, p => p.MapFrom(x => x.CreateTime.Date));
        }
    }

    public class TrackRawDataDto
    {
        public ProductEnum Product { get; set; }

        public int DataTrackingSceneId { get; set; }

        public DateTime CreateTime { get; set; }

    }

    public class TrackRawDataEntity
    {
        public int ProductId { get; set; }

        public int DataTrackingSceneId { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public enum ProductEnum
    {
        ERP = 1,
        CRM = 2,
        WMS = 3
    }
}

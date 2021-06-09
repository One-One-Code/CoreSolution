using AutoMapper;
using OneOne.Utility4Core.ExtensionsMethod;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    public class MapperTest
    {
        public  static void Test()
        {
            MapperExtension.CreateMapperByAssembly(new string[] { "ConsoleTest" });
            var entity = new TrackRawDataDto { Product = ProductEnum.ERP, CreateTime = DateTime.Now };
            var list = new List<TrackRawDataDto>();
            list.Add(entity);
            list.Add(entity);
            var result = MapperExtension.MapTo<TrackRawDataEntity, TrackRawDataDto>(entity);
            var result1 = MapperExtension.MapTo<TrackRawDataEntity, TrackRawDataDto>(list);
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

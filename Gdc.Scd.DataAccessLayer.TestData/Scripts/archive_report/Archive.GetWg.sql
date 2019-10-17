if OBJECT_ID('Archive.GetWg') is not null
    drop function Archive.GetWg;
go

create function [Archive].[GetWg](@software bit)
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
    , Sog nvarchar(255)
)
begin

    insert into @tbl
    select  wg.Id
          , wg.Name as Wg
          , wg.Description
          , pla.Name as Pla
          , cpla.Name as ClusterPla
          , sog.Name as Sog
    from InputAtoms.Wg wg 
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.Deactivated = 0
          and (@software is null or wg.IsSoftware = @software)

    return;

end
go
if OBJECT_ID('Archive.GetSwDigit') is not null
    drop function Archive.GetSwDigit;
go

create function [Archive].[GetSwDigit]()
returns @tbl table (
      Id bigint not null primary key
    , Name nvarchar(255)
    , Description nvarchar(255)
    , Sog nvarchar(255)
    , Sfab nvarchar(255)
    , Pla nvarchar(255)
    , ClusterPla nvarchar(255)
)
begin

    insert into @tbl
    select    dig.Id
            , dig.Name
            , dig.Description

            , sog.Name as Sog
            , sfab.Name as Sfab
            , pla.Name as Pla
            , cpla.Name as ClusterPla
    from InputAtoms.SwDigit dig
    left join InputAtoms.Sog sog on sog.Id = dig.SogId and sog.DeactivatedDateTime is null
    left join InputAtoms.Sfab sfab on sfab.Id = sog.SFabId and sfab.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = sfab.PlaId
    left join InputAtoms.ClusterPla cpla on cpla.Id = pla.ClusterPlaId

    where dig.DeactivatedDateTime is null

    return;

end
GO
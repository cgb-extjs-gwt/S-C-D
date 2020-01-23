alter VIEW [InputAtoms].[WgSogView] as 
    select wg.Id, wg.Alignment, wg.CreatedDateTime, wg.DeactivatedDateTime
		,wg.[Description], wg.ExistsInLogisticsDb, wg.FabGrp, wg.IsDeactivatedInLogistic
		,wg.IsSoftware, wg.ModifiedDateTime, wg.[Name], wg.PlaId, wg.RoleCodeId, wg.SCD_ServiceType 
		,wg.SFabId, wg.SogId, wg.WgType, wg.ResponsiblePerson, wg.CentralContractGroupId, wg.PsmRelease
		,wg.CompanyId, wg.ServiceTypes, wg.Deactivated, wg.IsNotified
        ,sog.Name as Sog
        ,sog.Description as SogDescription
    from InputAtoms.Wg wg
    left join InputAtoms.Sog sog on sog.id = wg.SogId
    where wg.DeactivatedDateTime is null
GO
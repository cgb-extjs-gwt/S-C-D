import { FilterState } from "../States/ApprovalState";
import { ApprovalBundleFilter } from "../Services/ApprovalService";

export const buildBundleFilter = (filterState: FilterState) => (<ApprovalBundleFilter>{
    dateTimeFrom: filterState.startDate || null,
    dateTimeTo: filterState.endDate || null,
    applicationIds: filterState.selectedApplicationId ? [ filterState.selectedApplicationId ] : null,
    costBlockIds: filterState.selectedCostBlockIds || null,
    costElementIds: filterState.selectedCostElementIds ? filterState.selectedCostElementIds.map(el => el.costElementId) : null,
    state: filterState.selectedState
})
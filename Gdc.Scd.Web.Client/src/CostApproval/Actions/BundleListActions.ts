import { BundleFilter } from "../States/BundleFilter";
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import * as approvalService from '../Services/CostApprovalService';
import { ApprovalBundle } from "../States/ApprovalBundle";
import { PageCommonAction } from "../../Common/Actions/CommonActions";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export const COST_APPROVAL_LOAD_BUNDLES = "COST_APPROVAL.LOAD_BUNDLES";

const buildLoadBundlesByFilterAction = (
    getBundlesFn: (filter: BundleFilter, approvalBundleState: ApprovalBundleState) => Promise<ApprovalBundle[]>
) => (pageName: string, approvalBundleState: ApprovalBundleState) =>
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const state = getState();
            const page = <ApprovalCostElementsLayoutState>state.pages[pageName];
            const applyFilter = page.filter;
            
            if (applyFilter) {
                const filter = <BundleFilter>{
                    dateTimeFrom: applyFilter.startDate || null,
                    dateTimeTo: applyFilter.endDate || null,
                    applicationIds: applyFilter.selectedApplicationId ? [ applyFilter.selectedApplicationId ] : null,
                    costBlockIds: applyFilter.selectedCostBlockIds || null,
                    costElementIds: applyFilter.selectedCostElementIds ? applyFilter.selectedCostElementIds.map(el => el.costElementId) : null
                }

                handleRequest(
                    getBundlesFn(filter, approvalBundleState).then(bundles => dispatch(<PageCommonAction<ApprovalBundle[]>>{
                        type: COST_APPROVAL_LOAD_BUNDLES,
                        data: bundles,
                        pageName
                    }))
                );
            }
        }
    )

export const loadBundlesByFilter = buildLoadBundlesByFilterAction(
    (filter, approvalBundleState) => approvalService.getBundles(filter, approvalBundleState)
)

export const loadOwnBundlesByFilter = buildLoadBundlesByFilterAction(
    (filter, approvalBundleState) => approvalService.getOwnBundles(filter, approvalBundleState)
)
import { CommonState } from "../../Layout/States/AppStates";
import { connectAdvanced } from "react-redux";
import FilterBundlesView, { FilterApprovalProps, ApprovalFilterActions, CheckedCostBlock, CheckedCostElement, CheckedItem } from "./FilterBundlesView";
import { NamedId, PageName } from "../../Common/States/CommonStates";
import { ItemSelectedAction, CommonAction, PageItemSelectedAction, PageCommonAction, PageAction, MultiItemSelectedAction, PageMultiItemSelectedAction } from '../../Common/Actions/CommonActions';
import * as approvalActions from '../../CostApproval/Actions/CostApprovalFilterActions';
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import { loadBundlesByFilter } from '../Actions/BundleListActions'
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { CostElementId, BundleFilterStates } from "../States/BundleFilterStates";
import { Dispatch } from "redux";
import { CostBlockMeta, CostElementMeta } from "../../Common/States/CostMetaStates";
import { mapCostElements } from "../../Common/Helpers/MetaHelper";

export interface FilterBundleContainerProps extends PageName {
    approvalBundleState: ApprovalBundleState
}

const getVisibleCostBlocks = (costBlocks: CostBlockMeta[], filter: BundleFilterStates) => costBlocks.filter(
    costBlockMeta => costBlockMeta.applicationIds.includes(filter.selectedApplicationId)
)

const isAllItemsChecked = (items: CheckedItem[]) => items.every(item => item.isChecked);

const getPageState = (state: CommonState, pageName: string) => <ApprovalCostElementsLayoutState>state.pages[pageName];

const sortByName = <T extends { name: string }>(items: T[]) => items.sort(
    (item1, item2) => item1.name.localeCompare(item2.name)
)

const buildProps = (state: CommonState, { pageName }: FilterBundleContainerProps) => {
    let props: FilterApprovalProps;

    if (state.app.appMetaData) {
        const meta = state.app.appMetaData;
        const page = getPageState(state, pageName);
        const { filter } = page;

        const costBlocks: CheckedCostBlock[] =[];
        const checkedCostBlockMetas: CostBlockMeta[] = [];
        const visibleCostBlocks = getVisibleCostBlocks(meta.costBlocks, filter);

        for (const costBlockMeta of visibleCostBlocks) {
            const isCheckedCostBlock = filter.selectedCostBlockIds.includes(costBlockMeta.id);

            costBlocks.push({
                id: costBlockMeta.id,
                name: costBlockMeta.name,
                isChecked: isCheckedCostBlock
            });

            if (isCheckedCostBlock) {
                checkedCostBlockMetas.push(costBlockMeta);
            }
        }

        const costElements = mapCostElements(
            checkedCostBlockMetas, 
            (costElementMeta, costBlockMeta) => (<CheckedCostElement>{
                costBlockId: costBlockMeta.id,
                costElementId: costElementMeta.id,
                name: costElementMeta.name,
                isChecked: !!filter.selectedCostElementIds.find(
                    ({ costBlockId, costElementId }) => 
                        costBlockMeta.id == costBlockId && costElementMeta.id == costElementId
                )
            })
        )

        return <FilterApprovalProps>{
            application: {
                selectedItemId: filter.selectedApplicationId,
                list: meta.applications
            },
            costBlocks: sortByName(costBlocks),
            costElements: sortByName(costElements),
            startDate: filter.startDate || new Date(),
            endDate: filter.endDate || new Date(),
            isAllCostBlocksChecked: isAllItemsChecked(costBlocks),
            isAllCostElementsChecked: isAllItemsChecked(costElements)
        }

    } else {
        props = {};
    }
}

const buildActions = (state: CommonState, { pageName, approvalBundleState }: FilterBundleContainerProps, dispatch: Dispatch) => (<ApprovalFilterActions>{
    onApplicationSelect: (selectedAppId) => dispatch(<PageItemSelectedAction>{
        type: approvalActions.COST_APPROVAL_SELECT_APPLICATION,
        selectedItemId: selectedAppId,
        pageName
    }),
    onCostBlockCheck: (selectedCostBlock) => dispatch(<PageItemSelectedAction>{
        type: approvalActions.COST_APPROVAL_CHECK_COST_BLOCK,
        selectedItemId: selectedCostBlock,
        pageName
    }),
    onCostBlockUncheck: (selectedCostBlock) => dispatch(<PageItemSelectedAction>{
        type: approvalActions.COST_APPROVAL_UNCHECK_COST_BLOCK,
        selectedItemId: selectedCostBlock,
        pageName
    }),
    onCostElementCheck: (costElementId, costBlockId) => dispatch(<PageCommonAction<CostElementId>>{
        type: approvalActions.COST_APPROVAL_CHECK_COST_ELEMENT,
        pageName,
        data: {
            costElementId,
            costBlockId
        }
    }),
    onCostElementUncheck: (costElementId, costBlockId) => dispatch(<PageCommonAction<CostElementId>>{
        type: approvalActions.COST_APPROVAL_UNCHECK_COST_ELEMENT,
        pageName,
        data: {
            costBlockId,
            costElementId
        }
    }),
    onShowAllCostBlocksCheck: isChecked => { 
        let costBlockIds: string[];

        if (isChecked) {
            const { filter } = getPageState(state, pageName);

            costBlockIds = getVisibleCostBlocks(state.app.appMetaData.costBlocks, filter).map(costBlock => costBlock.id);
        } else {
            costBlockIds = [];
        }

        dispatch(<PageMultiItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_CHECK_MULTI_COST_BLOCKS,
            pageName,
            selectedItemIds: costBlockIds
        })
    },
    onShowAllCostCostElementsCheck: isChecked => {
        let costElementIds: CostElementId[];

        if (isChecked) {
            const { filter } = getPageState(state, pageName);
            const visibleCostBlocks = getVisibleCostBlocks(state.app.appMetaData.costBlocks, filter)

            costElementIds = visibleCostBlocks.reduce<CostElementId[]>(
                (acc, costBlock) => acc.concat(
                    costBlock.costElements.map(costElement => (<CostElementId>{
                        costBlockId: costBlock.id,
                        costElementId: costElement.id
                    }))
                ), 
                []);
        } else {
            costElementIds = [];
        }

        dispatch({
            type: approvalActions.COST_APPROVAL_CHECK_MULTI_COST_ELEMENTS,
            pageName,
            data: costElementIds
        })
    },
    onStartDateChange: (selectedDate) => dispatch(<PageCommonAction<Date>>{
        type: approvalActions.COST_APPROVAL_SELECT_START_DATE,
        data: selectedDate,
        pageName
    }),
    onEndDateChange: (selectedDate) => dispatch(<PageCommonAction<Date>>{
        type: approvalActions.COST_APPROVAL_SELECT_END_DATE,
        data: selectedDate,
        pageName
    }),
    onApplyFilter: () => dispatch(loadBundlesByFilter(pageName, approvalBundleState))
})

export const FilterBundleContainer = connectAdvanced<CommonState, FilterApprovalProps, FilterBundleContainerProps>(
    dispatch => (state, ownProps) => ({
        ...buildProps(state, ownProps),
        ...buildActions(state, ownProps, dispatch)
    })
)(FilterBundlesView)
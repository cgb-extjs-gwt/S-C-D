import { CommonState } from "../../Layout/States/AppStates";
import { connectAdvanced } from "react-redux";
import { NamedId, PageName } from "../../Common/States/CommonStates";
import { ItemSelectedAction, CommonAction, PageItemSelectedAction, PageCommonAction, PageAction, MultiItemSelectedAction, PageMultiItemSelectedAction } from '../../Common/Actions/CommonActions';
import { Dispatch, Action } from "redux";
import { CostBlockMeta, CostElementMeta } from "../../Common/States/CostMetaStates";
import { mapCostElements } from "../../Common/Helpers/MetaHelper";
import { FilterState, ApprovalCostElementsLayoutState, CostElementId } from "../States/ApprovalState";
import { APPROVAL_FILTER_SELECT_APPLICATION, APPROVAL_FILTER_CHECK_COST_BLOCK, APPROVAL_FILTER_UNCHECK_COST_BLOCK, APPROVAL_FILTER_CHECK_COST_ELEMENT, APPROVAL_FILTER_UNCHECK_COST_ELEMENT, APPROVAL_FILTER_CHECK_MULTI_COST_BLOCKS, APPROVAL_FILTER_CHECK_MULTI_COST_ELEMENTS, APPROVAL_FILTER_SELECT_START_DATE, APPROVAL_FILTER_SELECT_END_DATE } from "../Actions/FilterActions";
import { CheckedItem, FilterProps, CheckedCostBlock, CheckedCostElement, FilterActions, FilterView } from "./FilterView";

const getVisibleCostBlocks = (costBlocks: CostBlockMeta[], filter: FilterState) => costBlocks.filter(
    costBlockMeta => costBlockMeta.applicationIds.includes(filter.selectedApplicationId)
)

const isAllItemsChecked = (items: CheckedItem[]) => items.every(item => item.isChecked);

const getPageState = (state: CommonState, pageName: string) => <ApprovalCostElementsLayoutState>state.pages[pageName];

const sortByName = <T extends { name: string }>(items: T[]) => items.sort(
    (item1, item2) => item1.name.localeCompare(item2.name)
)

const buildProps = (state: CommonState, { pageName }: PageName) => {
    let props: FilterProps;

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

        return <FilterProps>{
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

const buildActions = (state: CommonState, { pageName }: PageName, dispatch: Dispatch) => (<FilterActions>{
    onApplicationSelect: (selectedAppId) => dispatch(<PageItemSelectedAction>{
        type: APPROVAL_FILTER_SELECT_APPLICATION,
        selectedItemId: selectedAppId,
        pageName
    }),
    onCostBlockCheck: (selectedCostBlock) => dispatch(<PageItemSelectedAction>{
        type: APPROVAL_FILTER_CHECK_COST_BLOCK,
        selectedItemId: selectedCostBlock,
        pageName
    }),
    onCostBlockUncheck: (selectedCostBlock) => dispatch(<PageItemSelectedAction>{
        type: APPROVAL_FILTER_UNCHECK_COST_BLOCK,
        selectedItemId: selectedCostBlock,
        pageName
    }),
    onCostElementCheck: (costElementId, costBlockId) => dispatch(<PageCommonAction<CostElementId>>{
        type: APPROVAL_FILTER_CHECK_COST_ELEMENT,
        pageName,
        data: {
            costElementId,
            costBlockId
        }
    }),
    onCostElementUncheck: (costElementId, costBlockId) => dispatch(<PageCommonAction<CostElementId>>{
        type: APPROVAL_FILTER_UNCHECK_COST_ELEMENT,
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
            type: APPROVAL_FILTER_CHECK_MULTI_COST_BLOCKS,
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
            type: APPROVAL_FILTER_CHECK_MULTI_COST_ELEMENTS,
            pageName,
            data: costElementIds
        })
    },
    onStartDateChange: (selectedDate) => dispatch(<PageCommonAction<Date>>{
        type: APPROVAL_FILTER_SELECT_START_DATE,
        data: selectedDate,
        pageName
    }),
    onEndDateChange: (selectedDate) => dispatch(<PageCommonAction<Date>>{
        type: APPROVAL_FILTER_SELECT_END_DATE,
        data: selectedDate,
        pageName
    })
})

export const FilterContainer = connectAdvanced<CommonState, FilterProps, PageName>(
    dispatch => (state, ownProps) => ({
        ...buildProps(state, ownProps),
        ...buildActions(state, ownProps, dispatch)
    })
)(FilterView)
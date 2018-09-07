import { CommonState } from "../../Layout/States/AppStates";
import { connect } from "react-redux";
import FilterBundlesView, { FilterApprovalProps, ApprovalFilterActions } from "./FilterBundlesView";
import { SelectList, NamedId, ElementWithParent, PageName } from "../../Common/States/CommonStates";
import { ItemSelectedAction, ItemWithParentSelectedAction, CommonAction, PageItemSelectedAction, PageCommonAction, PageItemWithParentSelectedAction, PageAction } from '../../Common/Actions/CommonActions';
import * as approvalActions from '../../CostApproval/Actions/CostApprovalFilterActions';
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import { loadBundlesByFilter } from '../Actions/BundleListActions'
import { ApprovalBundleState } from "../States/ApprovalBundleState";

export interface FilterBundleContainerProps extends PageName {
    approvalBundleState: ApprovalBundleState
}

export const FilterBundleContainer = connect<FilterApprovalProps, ApprovalFilterActions, FilterBundleContainerProps, CommonState>(
    (state, { pageName }) => {

        //app meta data is not loaded yet
        if (!state.app.appMetaData)
        {
            return <FilterApprovalProps>{ }
        }
        
        const meta = state.app.appMetaData;
        //const filter = state.pages.costApproval.filter;
        const page = <ApprovalCostElementsLayoutState>state.pages[pageName];
        const filter = page.filter;

        const applications = {
            selectedItemId: filter.selectedApplicationId,
            list: meta.applications
        }

        //getting selected cost blocks from our state
        const costBlocksMeta = meta.costBlocks.filter(costBlock => {
            return costBlock.applicationIds.indexOf(filter.selectedApplicationId) > -1
        });

        //mapping selected costBlocks to our state
        const defaultCostBlock = costBlocksMeta[0];

        const costBlock: NamedId[] = costBlocksMeta.map(costBlock => <NamedId>{id: costBlock.id, name: costBlock.name });

        //getting selected cost blocks
        const selectedCostBlocks = filter.selectedCostBlockIds;

        const costBlocks = {
            selectedItemIds: selectedCostBlocks,
            list: costBlock
        }

        //getting selected cost elements from our state
        //using map/reduce as select many
        const costElementsMeta: ElementWithParent[] = costBlocksMeta.filter(costBlock => selectedCostBlocks.indexOf(costBlock.id) > -1)
                                .map(costBlock => ({
                                    cbId: costBlock.id,
                                    cbElems: costBlock.costElements
                                }))
                                .reduce((acc, elem) => {
                                    return acc.concat(
                                        {   cbId: elem.cbId, 
                                            cbelems: [...elem.cbElems]
                                        });
                                }, [])
                                .reduce((acc, elem) => {
                                    return acc.concat(
                                        ...elem.cbelems.map(e => { 
                                            return {
                                                parentId: elem.cbId, 
                                                element: {id: e.id, name: e.name}
                                            }})
                                    )
                                }, []);                         
                                
        const selectedCostElements = filter.selectedCostElementIds.map(item => item.element);
                                   
        const costElements = {
            selectedItemIds: selectedCostElements,
            list: costElementsMeta
        }

        return <FilterApprovalProps>{
            application: applications,
            costBlocks: costBlocks,
            costElements: costElements,
            startDate: filter.startDate || new Date(),
            endDate: filter.endDate || new Date(),
        }
    },
    (dispatch, { pageName, approvalBundleState }) => (<ApprovalFilterActions>{
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
        onCostElementCheck: (selectedCostElement, parentElementId) => dispatch(<PageItemWithParentSelectedAction>{
            type: approvalActions.COST_APPROVAL_CHECK_COST_ELEMENT,
            selectedItemId: selectedCostElement,
            selectedItemParentId: parentElementId,
            pageName
        }),
        onCostElementUncheck: (selectCostElement) => dispatch(<PageItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_UNCHECK_COST_ELEMENT,
            selectedItemId: selectCostElement,
            pageName
        }),
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
        // onApplyFilter: () => dispatch(<PageAction>{
        //     type: approvalActions.COST_APPROVAL_APPLY_FILTER,
        //     pageName
        // }),
        onApplyFilter: () => loadBundlesByFilter(pageName, approvalBundleState)
    })
    
)(FilterBundlesView)
import { CommonState } from "../../Layout/States/AppStates";
import { connect } from "react-redux";
import FilterBundlesView, { FilterApprovalProps, ApprovalFilterActions } from "./FilterBundlesView";
import { SelectList, NamedId, ElementWithParent } from "../../Common/States/CommonStates";
import { ItemSelectedAction, ItemWithParentSelectedAction, CommonAction } from '../../Common/Actions/CommonActions';
import * as approvalActions from '../../CostApproval/Actions/CostApprovalFilterActions';


export const FilterBundleContainer = connect<FilterApprovalProps, ApprovalFilterActions, {}, CommonState>
(
    state => {

        const applicationsMeta: NamedId[] = state.app.appMetaData ? state.app.appMetaData.applications : [];
        const selectedApplicationId = state.pages.costApproval.filter.selectedApplicationId;

        const applications = {
            selectedItemId: selectedApplicationId,
            list: applicationsMeta
        }

        //getting selected cost blocks from our state
        const costBlocksMeta = state.app.appMetaData ? state.app.appMetaData.costBlocks.filter(costBlock => {
            return costBlock.applicationIds.indexOf(selectedApplicationId) > -1
        }) : [];

        //mapping selected costBlocks to our state
        const costBlock: NamedId[] = costBlocksMeta.map(costBlock => <NamedId>{id: costBlock.id, name: costBlock.name });

        //getting selected cost blocks
        const selectedCostBlocks = state.pages.costApproval.filter.selectedCostBlockIds;

        const costBlocks = {
            selectedItemIds: selectedCostBlocks,
            list: costBlock
        }

        //getting selected cost elements from our state
        //using map/reduce as select many
        const costElementsMeta = costBlocksMeta.filter(costBlock => selectedCostBlocks.indexOf(costBlock.id) > -1)
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
                                
        const selectedCostElements = state.pages.costApproval.filter.selectedCostElementIds.map(item => item.element);

        const costElements = {
            selectedItemIds: selectedCostElements,
            list: costElementsMeta
        }

        return <FilterApprovalProps>{
            application: applications,
            costBlocks: costBlocks,
            costElements: costElements,
            startDate: state.pages.costApproval.filter.startDate ?  state.pages.costApproval.filter.startDate : new Date(),
            endDate: state.pages.costApproval.filter.endDate ? state.pages.costApproval.filter.endDate : new Date()
        }
    },
    dispatch => (<ApprovalFilterActions>{
        onApplicationSelect: (selectedAppId) => dispatch(<ItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_SELECT_APPLICATION,
            selectedItemId: selectedAppId
        }),
        onCostBlockCheck: (selectedCostBlock) => dispatch(<ItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_CHECK_COST_BLOCK,
            selectedItemId: selectedCostBlock
        }),
        onCostBlockUncheck: (selectedCostBlock) => dispatch(<ItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_UNCHECK_COST_BLOCK,
            selectedItemId: selectedCostBlock
        }),
        onCostElementCheck: (selectedCostElement, parentElementId) => dispatch(<ItemWithParentSelectedAction>{
            type: approvalActions.COST_APPROVAL_CHECK_COST_ELEMENT,
            selectedItemId: selectedCostElement,
            selectedItemParentId: parentElementId
        }),
        onCostElementUncheck: (selectCostElement) => dispatch(<ItemSelectedAction>{
            type: approvalActions.COST_APPROVAL_UNCHECK_COST_ELEMENT,
            selectedItemId: selectCostElement
        }),
        onStartDateChange: (selectedDate) => dispatch(<CommonAction<Date>>{
            type: approvalActions.COST_APPROVAL_SELECT_START_DATE,
            data: selectedDate
        }),
        onEndDateChange: (selectedDate) => dispatch(<CommonAction<Date>>{
            type: approvalActions.COST_APPROVAL_SELECT_END_DATE,
            data: selectedDate
        }),
        onApplyFilter: () => dispatch({
            type: approvalActions.COST_APPROVAL_APPLY_FILTER
        })
    })
    
)(FilterBundlesView)
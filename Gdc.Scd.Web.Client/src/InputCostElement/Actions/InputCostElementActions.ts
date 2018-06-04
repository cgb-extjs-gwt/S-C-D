import { PageActionBuilder } from "../../Layout/Actions/PageActions";
import { CostElementInputState } from "../States/CostElementState";
import { NamedId } from "../../Common/States/NamedId";
import { CommonAction } from "../../Common/CommonAction";
import { Action, Dispatch } from "redux";
import { getCostElementInput } from "../Services/CostElementService";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";

export const COST_ELEMENT_INTPUT_PAGE = 'CostElementsInputs';
export const COST_ELEMENT_INTPUT_SELECT_APPLICATION = 'COST_ELEMENT_INTPUT.SELECT.APPLICATION';
export const COST_ELEMENT_INTPUT_SELECT_SCOPE = 'COST_ELEMENT_INTPUT.SELECT.SCOPE';
export const COST_ELEMENT_INTPUT_SELECT_COST_BLOCK = 'COST_ELEMENT_INTPUT.SELECT.COST_BLOCK';

const actionBuilder = new PageActionBuilder(COST_ELEMENT_INTPUT_PAGE, 'Cost elements inputs');

export const init = () => asyncAction(
    dispatch => {
        dispatch(actionBuilder.openPage());
        getCostElementInput().then(
            costElement => dispatch(actionBuilder.initPageSuccess(costElement)),
            error => dispatch(actionBuilder.initPageError(error))
        );
    }
)

export const selectApplication = (selectedApplicationId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_APPLICATION,
    selectedItemId: selectedApplicationId
})

export const selectScope = (selectedScopeId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_SCOPE,
    selectedItemId: selectedScopeId
})

export const selectCostBlock = (selectedCostBlockId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_COST_BLOCK,
    selectedItemId: selectedCostBlockId
});
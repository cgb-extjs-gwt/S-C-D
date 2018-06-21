import { PageActionBuilder } from "../../Layout/Actions/PageActions";
import { CostEditorState } from "../States/CostEditorStates";
import { Action, Dispatch } from "redux";
import { getCostEditorDto } from "../Services/CostEditorServices";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { PageCommonState } from "../../Layout/States/PageStates";
import { losseDataCheckAction } from "../Helpers/CostEditorHelpers";

export const COST_ELEMENT_INTPUT_PAGE = 'CostElementsInputs';
export const COST_ELEMENT_INTPUT_SELECT_APPLICATION = 'COST_ELEMENT_INTPUT.SELECT.APPLICATION';
export const COST_ELEMENT_INTPUT_SELECT_SCOPE = 'COST_ELEMENT_INTPUT.SELECT.SCOPE';
export const COST_ELEMENT_INTPUT_SELECT_COST_BLOCK = 'COST_ELEMENT_INTPUT.SELECT.COST_BLOCK';
export const COST_ELEMENT_INTPUT_HIDE_LOSE_CHANGES_WARNING = 'COST_ELEMENT_INTPUT.HIDE.LOSE_CHANGES_WARNING';
export const COST_ELEMENT_INTPUT_SHOW_LOSE_CHANGES_WARNING = 'COST_ELEMENT_INTPUT.SHOW.LOSE_CHANGES_WARNING';
export const COST_ELEMENT_INTPUT_LOSE_CHANGES = 'COST_ELEMENT_INTPUT.LOSE.CHANGES';

export interface ShowDataLoseWarningAction extends Action<string> {
    dataLoseAction: Action<string>
}

const actionBuilder = new PageActionBuilder(COST_ELEMENT_INTPUT_PAGE, 'Cost elements inputs');

export const init = () => asyncAction(
    dispatch => {
        dispatch(actionBuilder.openPage());
        getCostEditorDto().then(
            costElement => dispatch(actionBuilder.initPageSuccess(costElement)),
            error => dispatch(actionBuilder.initPageError(error))
        );
    }
)

export const selectApplication = (applicationId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_APPLICATION,
    selectedItemId: applicationId
})

export const selectScope = (selectedScopeId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_SCOPE,
    selectedItemId: selectedScopeId
})

export const selectCostBlock = (selectedCostBlockId: string) => (<ItemSelectedAction>{
    type: COST_ELEMENT_INTPUT_SELECT_COST_BLOCK,
    selectedItemId: selectedCostBlockId
});

export const showDataLoseWarning = dataLoseAction => (<ShowDataLoseWarningAction>{
    type: COST_ELEMENT_INTPUT_SHOW_LOSE_CHANGES_WARNING,
    dataLoseAction
})

export const hideDataLoseWarning = () => (<Action<string>>{
    type: COST_ELEMENT_INTPUT_HIDE_LOSE_CHANGES_WARNING
})

export const loseChanges = () => asyncAction<PageCommonState<CostEditorState>>(
    (dispatch, getState) => {
        dispatch(hideDataLoseWarning());
        dispatch(<Action<string>>{
            type: COST_ELEMENT_INTPUT_LOSE_CHANGES
        })
        
        const { page } = getState();

        dispatch(page.data.dataLossInfo.action);
    }
)

export const selectApplicationLosseDataCheck = (applicationId: string) => losseDataCheckAction(
    selectApplication(applicationId)
)

export const selectScopeLosseDataCheck = (selectedScopeId: string) => losseDataCheckAction(
    selectScope(selectedScopeId)
)
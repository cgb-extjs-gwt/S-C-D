import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { getProjectItemEditData } from "../Services/ProjectService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { setProjectItemEditData } from "./ProjectCalculatorActions";

export const loadProjectItemEditData = () => asyncAction<CommonState>(
    (dispatch, getState) => {
        const { pages: { projectCalculator } } = getState();

        if (!projectCalculator.projectItemEditData) {
            handleRequest(
                getProjectItemEditData().then(
                    data => dispatch(setProjectItemEditData(data))
                )
            )
        }
    }
)
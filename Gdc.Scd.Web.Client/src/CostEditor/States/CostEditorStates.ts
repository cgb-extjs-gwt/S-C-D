import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId, SelectList } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

// export interface CostBlockSet {
//     [key: string]: CostBlockState
// }

// export interface ApplicationSet {
//     [key: string] : CostBlockSet
// }

export interface ApplicationState {
    id: string
    costBlocks: SelectList<CostBlockState>
}

export interface CostEditorState {
    // applications: Map<string, NamedId>
    // costBlockMetas: Map<string, CostBlockMeta>
    //selectedApplicationId: string
    //costBlocks: CostBlockState[]
    //visibleCostBlockIds: string[]
    //costBlocks: ApplicationSet
    // selectedCostBlockId: string
    applications: SelectList<ApplicationState>
    dataLossInfo: {
        isWarningDisplayed: boolean
        action: Action<string>
    }
}
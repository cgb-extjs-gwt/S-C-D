import { Reducer, Action } from "redux";
import { CostBlockInputState, CostElementInput } from "../States/CostBlock";
import { PAGE_INIT_SUCCESS, PageAction } from "../../Layout/Actions/PageActions";
import { CostElementInputDto, CostElementMeta, CostElementInputState, CostBlockMeta } from "../States/CostElementState";
import { 
    COST_ELEMENT_INTPUT_PAGE, 
    COST_ELEMENT_INTPUT_SELECT_APPLICATION, 
    COST_ELEMENT_INTPUT_SELECT_SCOPE, 
    COST_ELEMENT_INTPUT_SELECT_COST_BLOCK 
} from "../Actions/InputCostElementActions";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { 
    COST_BLOCK_INPUT_SELECT_COUNTRY, 
    COST_BLOCK_INPUT_SELECT_COST_ELEMENT,
    CountrySelectedAction, 
    CostElementAction, 
    CostBlockInputAction, 
    FilterSelectionChangedAction,
    COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER,
    CostElementFilterSelectionChangedAction,
    COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER,
    COST_BLOCK_INPUT_SELECT_INPUT_LEVEL,
    InputLevelAction,
    InputLevelFilterSelectionChangedAction,
    COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER,
    COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER,
    COST_BLOCK_INPUT_LOAD_COST_ELEMENT_FILTER,
    CostlElementsFilterLoadedAction,
    InputLevelFilterLoadedAction,
    COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER
 } from "../Actions/CostBlockInputActions";
import { mapIf } from "../../Common/Helpers/CommonHelpers";
import { changeSelecitonFilterItem, resetFilter, loadFilter } from "./FilterReducer";

const getVisibleCostBlockIds = (costBlockMetas: CostBlockMeta[], selectedApplicationId: string) => {
    return costBlockMetas.filter(costBlockMeta => costBlockMeta.applicationIds.includes(selectedApplicationId))
                         .map(costBlockMeta => costBlockMeta.id);
}

const getVisibleCostElementIds = (costElementMetas: CostElementMeta[], selectedScopeId: string) => {
    return costElementMetas.filter(costElementMeta => costElementMeta.scopeId === selectedScopeId)
                           .map(costElementMeta => costElementMeta.id);
}

const initSuccess: Reducer<CostElementInputState, PageAction<CostElementInputDto>> = (state, action) => {
    const { costBlockMetas, countries } = action.data;
    const selectedCountryId = countries[0].id;
    const visibleCostBlockIds = getVisibleCostBlockIds(costBlockMetas, state.selectedApplicationId);

    return action.pageName === COST_ELEMENT_INTPUT_PAGE 
        ? {
            ...state,
            costBlocksInputs: costBlockMetas.map(costBlockMeta => (<CostBlockInputState>{
                costBlockId: costBlockMeta.id,
                selectedCountryId,
                costElement: {
                    selectedItemId: null,
                    list: costBlockMeta.costElements.map(costElementMeta => (<CostElementInput>{
                        costElementId: costElementMeta.id
                    }))
                },
                visibleCostElementIds: getVisibleCostElementIds(costBlockMeta.costElements, state.selectedScopeId),
                inputLevel:{
                    selectedId: null,
                    filter: null
                },
                editItems: null
            })),
            visibleCostBlockIds,
            selectedCostBlockId: visibleCostBlockIds[0]
        } 
        : state;
}

const selectApplication: Reducer<CostElementInputState, ItemSelectedAction> = (state, action) => {
    const visibleCostBlockIds = getVisibleCostBlockIds(
        Array.from(state.costBlockMetas.values()), 
        action.selectedItemId);

    const selectedCostBlockId = visibleCostBlockIds.includes(state.selectedCostBlockId)
        ? state.selectedCostBlockId
        : visibleCostBlockIds[0];

    return {
        ...state,
        visibleCostBlockIds,
        selectedCostBlockId
    }
}

const selectScope: Reducer<CostElementInputState, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        costBlocksInputs: state.costBlocksInputs.map(costBlockInput => {
            const costBlockMeta = state.costBlockMetas.get(costBlockInput.costBlockId);
            const visibleCostElementIds = getVisibleCostElementIds(
                costBlockMeta.costElements, 
                action.selectedItemId);

            return {
                ...costBlockInput,
                visibleCostElementIds
            };
        })
    }
}

const buildCostBlockChanger = <TAction extends CostBlockInputAction>(
    changeFn: ((costBlock: CostBlockInputState, action: TAction) => CostBlockInputState)
) => 
        (state: CostElementInputState, action: Action<string>) => {
            const costBlockAction = <TAction>action;

            return <CostElementInputState>{
                ...state,
                costBlocksInputs: mapIf(
                    state.costBlocksInputs, 
                    costBlock => costBlock.costBlockId === costBlockAction.costBlockId,
                    costBlock => changeFn(costBlock, costBlockAction) 
                )
            }
        }

const buildCostElementListItemChanger = <TAction extends CostElementAction>(
    changeFn: ((costElement: CostElementInput, action: TAction) => CostElementInput)
) => 
    buildCostBlockChanger<TAction>(
        (costBlock, action) => ({
            ...costBlock,
            costElement: {
                ...costBlock.costElement,
                list: mapIf(
                    costBlock.costElement.list,
                    costElement => costElement.costElementId === action.costElementId,
                    costElement => changeFn(costElement, action)
                )
            }
        })
    )

const selectCountry = buildCostBlockChanger<CountrySelectedAction>(
    (costBlock, action) => ({...costBlock, selectedCountryId: action.countryId})
)

const selectCostElement = buildCostBlockChanger<CostElementAction>(
    (costBlock, action) => ({
        ...costBlock, 
        costElement: {
            ...costBlock.costElement,
            selectedItemId: action.costElementId
        }
    })
)

const changeSelectionCostElementFilter = buildCostElementListItemChanger<CostElementFilterSelectionChangedAction>(
    (costElement, action) => ({
        ...costElement,
        filter: changeSelecitonFilterItem(costElement.filter, action)
    })
)

const resetCostElementFilter = buildCostElementListItemChanger<CostElementAction>(
    costElement => ({
        ...costElement,
        filter: resetFilter(costElement.filter)
    })
)

const selectInputLevel = buildCostBlockChanger<InputLevelAction>(
    (costBlcok, action) => ({
        ...costBlcok,
        inputLevel: {
            ...costBlcok.inputLevel,
            selectedId: action.inputLevelId
        }
    })
)

const changeSelectionInputLevelFilter = buildCostBlockChanger<InputLevelFilterSelectionChangedAction>(
    (costBlcok, action) => ({
        ...costBlcok,
        inputLevel: {
            ...costBlcok.inputLevel,
            filter: changeSelecitonFilterItem(costBlcok.inputLevel.filter, action)
        }
    })
)

const resetInputLevelFilter = buildCostBlockChanger<InputLevelAction>(
    costBlcok => ({
        ...costBlcok,
        inputLevel: {
            ...costBlcok.inputLevel,
            filter: resetFilter(costBlcok.inputLevel.filter)
        }
    })
)

const loadCostElementFilter = buildCostElementListItemChanger<CostlElementsFilterLoadedAction>(
    (costElement, action) => ({
        ...costElement,
        filter: loadFilter(action.filterItems)
    })
)

const loadLevelInputFilter = buildCostBlockChanger<InputLevelFilterLoadedAction>(
    costBlcok => ({
        ...costBlcok,
        inputLevel: {
            ...costBlcok.inputLevel,
            filter: loadFilter(costBlcok.inputLevel.filter)
        }
    })
)

export const costBlockInputReducer: Reducer<CostElementInputState, Action<string>> = (state, action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostElementInputDto>>action)
        
        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action)

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return selectScope(state, <ItemSelectedAction>action)

        case COST_ELEMENT_INTPUT_SELECT_COST_BLOCK:
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_BLOCK_INPUT_SELECT_COUNTRY: 
            return selectCountry(state, action)

        case COST_BLOCK_INPUT_SELECT_COST_ELEMENT:
            return selectCostElement(state, action)

        case COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER: 
            return changeSelectionCostElementFilter(state, action)

        case COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER:
            return resetCostElementFilter(state, action)
            
        case COST_BLOCK_INPUT_SELECT_INPUT_LEVEL:
            return selectInputLevel(state, action)

        case COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER:
            return changeSelectionInputLevelFilter(state, action)

        case COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER:
            return resetInputLevelFilter(state, action);

        case COST_BLOCK_INPUT_LOAD_COST_ELEMENT_FILTER:
            return loadCostElementFilter(state, action)

        case COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER:
            return loadLevelInputFilter(state, action)

        default:
            return state;
    }
}
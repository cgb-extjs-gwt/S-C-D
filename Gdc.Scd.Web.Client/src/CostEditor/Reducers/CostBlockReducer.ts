import { Reducer, Action } from "redux";
import { CostBlockState, CostElementState, InputLevelState, CheckItem } from "../States/CostBlockStates";
import { PAGE_INIT_SUCCESS, PageAction } from "../../Layout/Actions/PageActions";
import { CostEdirotDto, CostElementMeta, CostEditorState, CostBlockMeta } from "../States/CostEditorStates";
import { 
    COST_ELEMENT_INTPUT_PAGE, 
    COST_ELEMENT_INTPUT_SELECT_APPLICATION, 
    COST_ELEMENT_INTPUT_SELECT_SCOPE, 
    COST_ELEMENT_INTPUT_SELECT_COST_BLOCK 
} from "../Actions/CostEditorActions";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { 
    COST_BLOCK_INPUT_SELECT_COUNTRY, 
    COST_BLOCK_INPUT_SELECT_COST_ELEMENT,
    CountrySelectedAction, 
    CostElementAction, 
    CostBlockAction, 
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
    COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER,
    COST_BLOCK_INPUT_LOAD_EDIT_ITEMS,
    EditItemsAction,
    COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS,
    ItemEditedAction,
    COST_BLOCK_INPUT_EDIT_ITEM,
    COST_BLOCK_INPUT_SAVE_EDIT_ITEMS,
    COST_BLOCK_INPUT_APPLY_FILTERS
 } from "../Actions/CostBlockActions";
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

const clearCostBlockFilters  = (costBlock: CostBlockState, clearSelected: boolean = false) => {
    const result: CostBlockState = {
        ...costBlock,
        costElement: {
            ...costBlock.costElement,
            list: costBlock.costElement.list.map(costElement => ({
                ...costElement,
                filter: null
            }))
        },
        inputLevel: {
            ...costBlock.inputLevel,
            list: costBlock.inputLevel.list.map(inputLevel => ({
                ...inputLevel,
                filter: null
            }))
        }
    };

    if (clearSelected) {
        result.costElement.selectedItemId = null;
        result.inputLevel.selectedItemId = null;
    }

    return result;
}

const initSuccess: Reducer<CostEditorState, PageAction<CostEdirotDto>> = (state, action) => {
    const { costBlockMetas, countries, inputLevels } = action.data;
    const selectedCountryId = countries[0].id;
    const visibleCostBlockIds = getVisibleCostBlockIds(costBlockMetas, state.selectedApplicationId);

    return action.pageName === COST_ELEMENT_INTPUT_PAGE 
        ? {
            ...state,
            costBlocks: costBlockMetas.map(costBlockMeta => (<CostBlockState>{
                costBlockId: costBlockMeta.id,
                selectedCountryId,
                costElement: {
                    selectedItemId: null,
                    list: costBlockMeta.costElements.map(costElementMeta => (<CostElementState>{
                        costElementId: costElementMeta.id
                    }))
                },
                visibleCostElementIds: getVisibleCostElementIds(costBlockMeta.costElements, state.selectedScopeId),
                inputLevel: {
                    selectedItemId: null,
                    list: inputLevels.map(inputLevel => (<InputLevelState>{
                        inputLevelId: inputLevel.id
                    }))
                },
                edit: {
                    originalItems: null,
                    editedItems: [],
                    isFiltersApplied: true,
                    appliedFilter: {
                        costElementsItemIds: new Set<string>(),
                        inputLevelItemIds: new Set<string>()
                    }
                }
            })),
            visibleCostBlockIds,
            selectedCostBlockId: visibleCostBlockIds[0]
        } 
        : state;
}

const selectApplication: Reducer<CostEditorState, ItemSelectedAction> = (state, action) => {
    const visibleCostBlockIds = getVisibleCostBlockIds(
        Array.from(state.costBlockMetas.values()), 
        action.selectedItemId);

    const selectedCostBlockId = visibleCostBlockIds.includes(state.selectedCostBlockId)
        ? state.selectedCostBlockId
        : visibleCostBlockIds[0];

    return {
        ...state,
        costBlocks: state.costBlocks.map(costBlock => clearCostBlockFilters(costBlock, true)),
        visibleCostBlockIds,
        selectedCostBlockId,
    }
}

const selectScope: Reducer<CostEditorState, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        costBlocks: state.costBlocks.map(costBlock => {
            const costBlockMeta = state.costBlockMetas.get(costBlock.costBlockId);
            const visibleCostElementIds = getVisibleCostElementIds(
                costBlockMeta.costElements, 
                action.selectedItemId);

            return {
                ...clearCostBlockFilters(costBlock, true),
                visibleCostElementIds
            };
        })
    }
}

const buildCostBlockChanger = <TAction extends CostBlockAction>(
    changeFn: ((costBlock: CostBlockState, action: TAction) => CostBlockState)
) => 
        (state: CostEditorState, action: Action<string>) => {
            const costBlockAction = <TAction>action;

            return <CostEditorState>{
                ...state,
                costBlocks: mapIf(
                    state.costBlocks, 
                    costBlock => costBlock.costBlockId === costBlockAction.costBlockId,
                    costBlock => changeFn(costBlock, costBlockAction) 
                )
            }
        }

const buildCostElementListItemChanger = <TAction extends CostElementAction>(
    costElementFn: ((costElement: CostElementState, action: TAction) => CostElementState)
) => 
    buildCostBlockChanger<TAction>(
        (costBlock, action) => ({
            ...costBlock,
            costElement: {
                ...costBlock.costElement,
                list: mapIf(
                    costBlock.costElement.list,
                    costElement => costElement.costElementId === action.costElementId,
                    costElement => costElementFn(costElement, action)
                )
            }
        })
    )

const buildInputLevelFilterChanger = <TAction extends InputLevelAction>(
    inputLevelFn: ((costElement: InputLevelState, action: TAction) => InputLevelState)
) => 
    buildCostBlockChanger<TAction>(
        (costBlock, action) => ({
            ...costBlock,
            inputLevel: {
                ...costBlock.inputLevel,
                list: mapIf(
                    costBlock.inputLevel.list, 
                    item => item.inputLevelId === action.inputLevelId, 
                    item => inputLevelFn(item, action)
                )
            }
        })
    )

const selectCountry = buildCostBlockChanger<CountrySelectedAction>(
    (costBlock, action) => ({
        ...clearCostBlockFilters(costBlock), 
        selectedCountryId: action.countryId,
    })
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
            selectedItemId: action.inputLevelId
        }
    })
)

const changeSelectionInputLevelFilter = buildInputLevelFilterChanger<InputLevelFilterSelectionChangedAction>(
    (inputLevel, action) => ({
        ...inputLevel, 
        filter: changeSelecitonFilterItem(inputLevel.filter, action)
    })
)

const resetInputLevelFilter = buildInputLevelFilterChanger(
    inputLevel => ({
        ...inputLevel,
        filter: resetFilter(inputLevel.filter)
    })
)

const loadCostElementFilter = buildCostElementListItemChanger<CostlElementsFilterLoadedAction>(
    (costElement, action) => ({
        ...costElement,
        filter: loadFilter(action.filterItems)
    })
)

const loadLevelInputFilter = buildInputLevelFilterChanger<InputLevelFilterLoadedAction>(
    (inputLevel, action) => ({
        ...inputLevel,
        filter: loadFilter(action.filterItems)
    })
)

const loadEditItems = buildCostBlockChanger<EditItemsAction>(
    (costBlock, action) => ({
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: [],
            originalItems: action.editItems
        }
    })
)

const clearEditItems = buildCostBlockChanger(
    costblock => ({
        ...costblock,
        edit: {
            ...costblock.edit,
            editedItems: []
        }
    })
)

const editItem = buildCostBlockChanger<ItemEditedAction>(
    (costBlock, action) => {
        const editedItems = costBlock.edit.editedItems;

        return {
            ...costBlock,
            edit: {
                ...costBlock.edit,
                editedItems: editedItems.filter(item => item.id !== action.item.id)
                                        .concat(action.item)
            }
        }
    }
)

const saveEditItems = buildCostBlockChanger(
    (costBlock, action) => ({
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: [],
            originalItems: costBlock.edit.originalItems.map(
                origItem => 
                    costBlock.edit.editedItems.find(editedItem => editedItem.id === origItem.id) || 
                    origItem
            )
        }
    })
)

const checkedFilterItemsSet = (filterItems: CheckItem[]) => {
    const ids = filterItems && filterItems.filter(item => item.isChecked).map(item => item.id);

    return new Set<string>(ids);
}

const applyFilters = buildCostBlockChanger(
    costBlock => {
        const { costElement, inputLevel } = costBlock;
        
        const selectedCostElement = 
            costElement.list.find(item => item.costElementId === costElement.selectedItemId);

        const selectInputLevel = 
            inputLevel.list.find(item => item.inputLevelId === inputLevel.selectedItemId);

        return <CostBlockState>{ 
            ...costBlock,
            edit: {
                ...costBlock.edit,
                appliedFilter: {
                    ...costBlock.edit.editedItems,
                    costElementsItemIds: checkedFilterItemsSet(selectedCostElement.filter),
                    inputLevelItemIds: checkedFilterItemsSet(selectInputLevel.filter)
                }
            }
        }
    }
)

export const costBlockReducer: Reducer<CostEditorState, Action<string>> = (state, action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostEdirotDto>>action)
        
        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action)

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return selectScope(state, <ItemSelectedAction>action)

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

        case COST_BLOCK_INPUT_LOAD_EDIT_ITEMS:
            return loadEditItems(state, action)

        case COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS:
            return clearEditItems(state, action)

        case COST_BLOCK_INPUT_EDIT_ITEM:
            return editItem(state, action)

        case COST_BLOCK_INPUT_SAVE_EDIT_ITEMS:
            return saveEditItems(state, action)

        case COST_BLOCK_INPUT_APPLY_FILTERS:
            return applyFilters(state, action)

        default:
            return state;
    }
}
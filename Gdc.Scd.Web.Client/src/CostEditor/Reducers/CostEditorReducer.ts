import { Action, Reducer } from "redux";
import { CostEditorState } from "../States/CostEditorStates";
import { 
    COST_EDITOR_PAGE, 
    COST_EDITOR_SELECT_APPLICATION, 
    COST_EDITOR_SELECT_COST_BLOCK,
    COST_EDITOR_HIDE_LOSE_CHANGES_WARNING,
    COST_EDITOR_LOSE_CHANGES,
    COST_EDITOR_SHOW_LOSE_CHANGES_WARNING,
    ShowDataLoseWarningAction
} from "../Actions/CostEditorActions";
import { CostBlockState, CostElementState, InputLevelState, CheckItem, EditItem, CostBlockEditState } from "../States/CostBlockStates";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { APP_PAGE_INIT, PageInitAction, LoadingAppDataAction, APP_LOAD_DATA } from "../../Layout/Actions/AppActions";
import { CountryInputLevelName } from "../../Common/Constants/MetaConstants";
import { CostBlockMeta, InputLevelMeta, CostMetaData, UsingInfo, FieldType } from "../../Common/States/CostMetaStates";
import { filterCostEditorItems } from "../Helpers/CostEditorHelpers";
import { COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA, CostBlockAction, CostElementAction, InputLevelAction, RegionSelectedAction, CostElementFilterSelectionChangedAction, InputLevelFilterSelectionChangedAction, CostElementDataLoadedAction, InputLevelFilterLoadedAction, EditItemsAction, ItemEditedAction, SaveEditItemsAction, COST_BLOCK_INPUT_SELECT_REGIONS, COST_BLOCK_INPUT_SELECT_COST_ELEMENT, COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER, COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER, COST_BLOCK_INPUT_SELECT_INPUT_LEVEL, COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_LOAD_EDIT_ITEMS, COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS, COST_BLOCK_INPUT_EDIT_ITEM, COST_BLOCK_INPUT_SAVE_EDIT_ITEMS, COST_BLOCK_INPUT_APPLY_FILTERS, COST_BLOCK_INPUT_RESET_ERRORS } from "../Actions/CostBlockActions";
import { mapIf } from "../../Common/Helpers/CommonHelpers";
import { changeSelecitonFilterItem, resetFilter, loadFilter } from "./FilterReducer";
import { findMeta } from "../../Common/Helpers/MetaHelper";

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

const initByMeta: Reducer<CostEditorState, LoadingAppDataAction> = (state, { data: { meta } }) => {
    // const { costBlocks } = action.data;
    // const applications = filterCostEditorItems(action.data.applications);
    // const selectedApplicationId = applications[0].id;
    // const costBlockMetas = filterCostEditorItems(costBlocks).map(costBlock => (<CostBlockMeta>{
    //     ...costBlock,
    //     costElements: filterCostEditorItems(costBlock.costElements).map(costElement => ({
    //         ...costElement,
    //         inputLevels: costElement.inputLevels.map((inputLevel, index) => (<InputLevelMeta>{
    //             ...inputLevel,
    //             levelNumer: index,
    //             isFilterLoading: index > 0 && costElement.inputLevels[index - 1].id != CountryInputLevelName
    //         }))
    //     }))
    // }))

    // return action.pageId === COST_EDITOR_PAGE 
    //     ? {
    //         ...state,
    //         applications: createMap(applications),
    //         regions: createMap([]),
    //         costBlockMetas: createMap(costBlockMetas),
    //         selectedApplicationId
    //     } 
    //     : state;


    // const applications = filterCostEditorItems(action.data.applications); 
    // const costBlocks = filterCostEditorItems(action.data.costBlocks);

    const costBlocks = filterCostEditorItems(meta.costBlocks).map(costBlockMeta => (<CostBlockState>{
        costBlockId: costBlockMeta.id,
        selectedRegionId: null,
        costElements: {
            selectedItemId: null,
            list: filterCostEditorItems(costBlockMeta.costElements).map(costElementMeta => (<CostElementState>{
                costElementId: costElementMeta.id,
                inputLevel: {
                    selectedItemId: null,
                    list: costElementMeta.inputLevels.map(inputLevel => (<InputLevelState>{
                        inputLevelId: inputLevel.id
                    }))
                },
                isDataLoaded: costElementMeta.regionInput || 
                              costElementMeta.dependency || 
                              costElementMeta.typeOptions && costElementMeta.typeOptions.Type === FieldType.Reference
            }))
        },
        //visibleCostElementIds: getVisibleCostElementIds(costBlockMeta),
        edit: {
            originalItems: null,
            editedItems: [],
            isFiltersApplied: true,
            appliedFilter: {
                costElementsItemIds: new Set<string>(),
                inputLevelItemIds: new Set<string>()
            },
            saveErrors: []
        }
    }));

    return {
        ...state,
        selectedApplicationId: filterCostEditorItems(meta.applications)[0].id,
        selectedCostBlockId: costBlocks[0].costBlockId,
        costBlocks
    };
}

const defaultState = () => (<CostEditorState>{
    dataLossInfo: {
        isWarningDisplayed: false,
        action: null,
    }
})

const showDataLoseWarning: Reducer<CostEditorState, ShowDataLoseWarningAction> = (state, action) => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: true,
        action: action.dataLoseAction
    }
})

const hideDataLoseWarning: Reducer<CostEditorState> = state => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: false,
        action: null
    }
})

const loseChanges: Reducer<CostEditorState, Action<string>> = state => ({
    ...state,
    costBlocks: state.costBlocks.map(costBlock => (<CostBlockState>{
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: []
        }
    })),
    dataLossInfo: {
        ...state.dataLossInfo,
    }
})

const clearCostBlockFilters  = (costBlock: CostBlockState, clearSelected: boolean = false) => {
    const result: CostBlockState = {
        ...costBlock,
        costElements: {
            ...costBlock.costElements,
            list: costBlock.costElements.list.map(costElement => ({
                ...costElement,
                filter: null,
                inputLevel: {
                    ...costElement.inputLevel,
                    list: costElement.inputLevel.list.map(inputLevel => ({
                        ...inputLevel,
                        filter: null
                    })),
                    selectedItemId: clearSelected ? null : costElement.inputLevel.selectedItemId,
                }
            }))
        }
    };

    if (clearSelected) {
        result.costElements.selectedItemId = null;
    }

    return result;
}

const selectApplication: Reducer<CostEditorState, ItemSelectedAction> = (state, action) => {
    // const visibleCostBlockIds = getVisibleCostBlockIds(
    //     Array.from(state.costBlockMetas.values()), 
    //     action.selectedItemId);

    // const selectedCostBlockId = visibleCostBlockIds.includes(state.selectedCostBlockId)
    //     ? state.selectedCostBlockId
    //     : visibleCostBlockIds[0];

    // return {
    //     ...state,
    //     costBlocks: state.costBlocks.map(costBlock => clearCostBlockFilters(costBlock, true)),
    //     visibleCostBlockIds,
    //     selectedCostBlockId,
    // }

    return {
        ...state,
        selectedApplicationId: action.selectedItemId,
        costBlocks: state.costBlocks.map(costBlock => clearCostBlockFilters(costBlock, true)),
    }
}

const buildCostBlockChanger = <TAction extends CostBlockAction, TParam=any>(
    changeFn: ((costBlock: CostBlockState, action: TAction, state: CostEditorState, addionalParam?: TParam) => CostBlockState)
) => 
        (state: CostEditorState, action: Action<string>, addionalParam?: TParam) => {
            const costBlockAction = <TAction>action;

            return <CostEditorState>{
                ...state,
                costBlocks: mapIf(
                    state.costBlocks, 
                    costBlock => costBlock.costBlockId === costBlockAction.costBlockId,
                    costBlock => changeFn(costBlock, costBlockAction, state, addionalParam) 
                )
            }
        }

const buildCostElementListItemChanger = <TAction extends CostElementAction>(
    costElementFn: ((costElement: CostElementState, action: TAction) => CostElementState)
) => 
    buildCostBlockChanger<TAction>(
        (costBlock, action) => ({
            ...costBlock,
            costElements: {
                ...costBlock.costElements,
                list: mapIf(
                    costBlock.costElements.list,
                    costElement => costElement.costElementId === action.costElementId,
                    costElement => costElementFn(costElement, action)
                )
            }
        })
    )

const buildInputLevelFilterChanger = <TAction extends InputLevelAction>(
    inputLevelFn: ((costElement: InputLevelState, action: TAction) => InputLevelState)
) => 
    buildCostElementListItemChanger<TAction>(
        (costElement, action) => ({
            ...costElement,
            inputLevel: {
                ...costElement.inputLevel,
                list: mapIf(
                    costElement.inputLevel.list, 
                    item => item.inputLevelId === action.inputLevelId, 
                    item => inputLevelFn(item, action)
                )
            }
        })
    )

const selectRegion = buildCostBlockChanger<RegionSelectedAction>(
    (costBlock, action) => ({
        ...clearCostBlockFilters(costBlock), 
        costElements: {
            ...costBlock.costElements,
            list: costBlock.costElements.list.map(
                costElement => 
                    costElement.costElementId === action.costElementId 
                        ? {
                            ...costElement,
                            region: {
                                ...costElement.region,
                                selectedItemId: action.regionId
                            }
                        }
                        : costElement
            )
        }
    })
)

// const selectCostElement = buildCostBlockChanger<CostElementAction, CostMetaData>(
//     (costBlock, { costElementId }, state, meta) => (<CostBlockState>{
//         ...costBlock, 
//         costElement: {
//             ...costBlock.costElement,
//             selectedItemId: costElementId,
//             list: costBlock.costElement.list.map(item => {
//                 if (item.costElementId === costElementId && !item.dataLoadingState) {
//                     const costBlockMeta = findMeta(meta.costBlocks, costBlock.costBlockId);
//                     const costElementMeta = costBlockMeta.costElements.find(item => item.id === costElementId);
//                     const isDataLoading = 
//                         costElementMeta.regionInput || 
//                         costElementMeta.dependency || 
//                         costElementMeta.typeOptions && costElementMeta.typeOptions.Type === FieldType.Reference;

//                     item = {
//                         ...item,
//                         dataLoadingState: isDataLoading ? DataLoadingState.WaitLoading : DataLoadingState.WithoutLoading
//                     }
//                 }

//                 return item;
//             })
//         }
//     })
// )

const selectCostElement = buildCostBlockChanger<CostElementAction>(
    (costBlock, { costElementId }, state) => (<CostBlockState>{
        ...costBlock, 
        costElements: {
            ...costBlock.costElements,
            selectedItemId: costElementId,
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

const selectInputLevel = buildCostElementListItemChanger<InputLevelAction>(
    (costElement, action) => ({
        ...costElement,
        inputLevel: {
            ...costElement.inputLevel,
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

const loadCostElementData = buildCostElementListItemChanger<CostElementDataLoadedAction>(
    (costElement, { costElementData }) => ({
        ...costElement,
        //dataLoadingState: DataLoadingState.Loaded,
        isDataLoaded: true,
        region: {
            ...costElement.region,
            list: costElementData.regions && costElementData.regions.sort((region1, region2) => region1.name.localeCompare(region2.name)),
            selectedItemId: costElementData.regions && costElementData.regions[0].id
        },
        filter: loadFilter(costElementData.filters),
        referenceValues: costElementData.referenceValues
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
        const editedItem = <EditItem>{
            ...action.item,
            valueCount: 1
        };

        return {
            ...costBlock,
            edit: {
                ...costBlock.edit,
                editedItems: editedItems.filter(item => item.id !== action.item.id)
                                        .concat(editedItem)
            }
        }
    }
)

const saveEditItems = buildCostBlockChanger<SaveEditItemsAction>(
    (costBlock, action) => {
        const edit: CostBlockEditState = action.qualityGateResult.hasErrors
            ? {
                ...costBlock.edit,
                saveErrors: action.qualityGateResult.errors
            }
            : {
                ...costBlock.edit,
                editedItems: [],
                originalItems: costBlock.edit.originalItems.map(
                    origItem => 
                        costBlock.edit.editedItems.find(editedItem => editedItem.id === origItem.id) || 
                        origItem
                ),
                saveErrors: []
            };

        return {
            ...costBlock,
            edit
        };
    }
)

const checkedFilterItemsSet = (filterItems: CheckItem[]) => {
    const ids = filterItems && filterItems.filter(item => item.isChecked).map(item => item.id);

    return new Set<string>(ids);
}

const applyFilters = buildCostBlockChanger(
    costBlock => {
        const { costElements } = costBlock;
        
        const selectedCostElement = 
            costElements.list.find(item => item.costElementId === costElements.selectedItemId);

        const selectInputLevel = 
            selectedCostElement.inputLevel.list.find(item => item.inputLevelId === selectedCostElement.inputLevel.selectedItemId);

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

const resetErrors = buildCostBlockChanger(
    costBlock => ({
        ...costBlock,
        edit: {
            ...costBlock.edit,
            saveErrors: []
        }
    })
)

export const costEditorReducer: Reducer<CostEditorState, Action<string>> = (state = defaultState(), action) => {
    switch(action.type) {
        // case APP_PAGE_INIT:
        //     return initSuccess(state, <PageInitAction<CostMetaData>>action);

        case APP_LOAD_DATA:
            return initByMeta(state, <LoadingAppDataAction>action);

        case COST_EDITOR_SELECT_APPLICATION:
            // return {
            //     ...state,
            //     selectedApplicationId: (<ItemSelectedAction>action).selectedItemId
            // }
            return selectApplication(state, <ItemSelectedAction>action)

        case COST_EDITOR_SELECT_COST_BLOCK:
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_EDITOR_SHOW_LOSE_CHANGES_WARNING:
            return showDataLoseWarning(state, <ShowDataLoseWarningAction>action);

        case COST_EDITOR_HIDE_LOSE_CHANGES_WARNING:
            return hideDataLoseWarning(state, action);

        case COST_EDITOR_LOSE_CHANGES:
            return loseChanges(state, action);

        case COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA:
            return loadCostElementData(state, action);

        case COST_BLOCK_INPUT_SELECT_REGIONS: 
            return selectRegion(state, action)

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

        case COST_BLOCK_INPUT_RESET_ERRORS:
            return resetErrors(state, action);

        default:
            return state;
    }
}


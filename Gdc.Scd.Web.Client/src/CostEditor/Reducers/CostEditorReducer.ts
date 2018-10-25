import { Action, Reducer } from "redux";
import { CostEditorState, ApplicationState } from "../States/CostEditorStates";
import { COST_EDITOR_PAGE, COST_EDITOR_SELECT_APPLICATION } from "../Actions/CostEditorActions";
import { CostBlockState, CostElementState, InputLevelState, CheckItem, EditItem, CostBlockEditState } from "../States/CostBlockStates";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { APP_PAGE_INIT, PageInitAction, LoadingAppDataAction, APP_LOAD_DATA } from "../../Layout/Actions/AppActions";
import { CountryInputLevelName } from "../../Common/Constants/MetaConstants";
import { CostBlockMeta, InputLevelMeta, CostMetaData, UsingInfo, FieldType } from "../../Common/States/CostMetaStates";
import { filterCostEditorItems, findCostBlockByState, findCostBlock, findApplication } from "../Helpers/CostEditorHelpers";
import { COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA, CostBlockAction, CostElementAction, InputLevelAction, RegionSelectedAction, CostElementFilterSelectionChangedAction, InputLevelFilterSelectionChangedAction, CostElementDataLoadedAction, InputLevelFilterLoadedAction, EditItemsAction, ItemEditedAction, SaveEditItemsAction, COST_BLOCK_INPUT_SELECT_REGIONS, COST_BLOCK_INPUT_SELECT_COST_ELEMENT, COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER, COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER, COST_BLOCK_INPUT_SELECT_INPUT_LEVEL, COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER, COST_BLOCK_INPUT_LOAD_EDIT_ITEMS, COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS, COST_BLOCK_INPUT_EDIT_ITEM, COST_BLOCK_INPUT_SAVE_EDIT_ITEMS, COST_BLOCK_INPUT_APPLY_FILTERS, COST_BLOCK_INPUT_RESET_ERRORS, COST_EDITOR_SELECT_COST_BLOCK } from "../Actions/CostBlockActions";
import { mapIf } from "../../Common/Helpers/CommonHelpers";
import { changeSelecitonFilterItem, resetFilter, loadFilter } from "./FilterReducer";
import { findMeta } from "../../Common/Helpers/MetaHelper";

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

const buildCostBlockState = (costBlockMeta: CostBlockMeta) => (<CostBlockState>{
    costBlockId: costBlockMeta.id,
    selectedRegionId: null,
    costElements: {
        selectedItemId: null,
        list: filterCostEditorItems(costBlockMeta.costElements).map(costElementMeta => (<CostElementState>{
            costElementId: costElementMeta.id,
            inputLevels: {
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
})

const initByMeta: Reducer<CostEditorState, LoadingAppDataAction> = (state, { data: { meta } }) => {
    const appMetas = filterCostEditorItems(meta.applications);
    const costBockMetas = filterCostEditorItems(meta.costBlocks);

    return <CostEditorState>{
        ...state,
        applications: {
            selectedItemId: appMetas[0].id,
            list: appMetas.map(appMeta => {
                const appCostBlocks = costBockMetas.filter(
                    costBlockMeta => costBlockMeta.applicationIds.includes(appMeta.id)
                );

                return <ApplicationState>{
                    id: appMeta.id,
                    costBlocks: {
                        selectedItemId: appCostBlocks[0].id,
                        list: appCostBlocks.map(buildCostBlockState)
                    }
                }
            })
        }
    };
}

const defaultState = () => (<CostEditorState>{
    applications: null,
    dataLossInfo: {
        isWarningDisplayed: false,
        action: null,
    }
})

const loseChanges: Reducer<CostEditorState, Action<string>> = (state, action) => {
    const { id: applicationId, costBlocks } = findApplication(state);

    state = mapCostBlock(state, applicationId, costBlocks.selectedItemId, costBlock => ({
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: []
        }
    }));

    return {
        ...state,
        dataLossInfo: {
            ...state.dataLossInfo,
        }
    }
}

const clearCostBlockFilters  = (costBlock: CostBlockState, clearSelected: boolean = false) => {
    const result: CostBlockState = {
        ...costBlock,
        costElements: {
            ...costBlock.costElements,
            list: costBlock.costElements.list.map(costElement => ({
                ...costElement,
                filter: null,
                inputLevel: {
                    ...costElement.inputLevels,
                    list: costElement.inputLevels.list.map(inputLevel => ({
                        ...inputLevel,
                        filter: null
                    })),
                    selectedItemId: clearSelected ? null : costElement.inputLevels.selectedItemId,
                }
            }))
        }
    };

    if (clearSelected) {
        result.costElements.selectedItemId = null;
    }

    return result;
}

const selectApplication: Reducer<CostEditorState, ItemSelectedAction> = (state, { selectedItemId }) => {
    return {
        ...state,
        applications: {
            ...state.applications,
            selectedItemId
        }
    }
}

const mapApplication = (
    state: CostEditorState, 
    applicationId: string,
    changeFn: (costBlock: ApplicationState) => ApplicationState
) => (<CostEditorState>{
    ...state,
    applications: {
        ...state.applications,
        list: mapIf(state.applications.list, app => app.id == applicationId, changeFn)
    }
})

const mapCostBlock = (
    state: CostEditorState,
    applicationId: string, 
    costBlockId: string, 
    changeFn: (costBlock: CostBlockState) => CostBlockState
) => mapApplication(state, applicationId, app => (<ApplicationState>{
    ...app,
    costBlocks: {
        ...app.costBlocks,
        list: mapIf(
            app.costBlocks.list, 
            costBlock => costBlock.costBlockId == costBlockId,
            changeFn
        )
    }
}))

const buildCostBlockChanger = <TAction extends CostBlockAction, TParam=any>(
    changeFn: ((costBlock: CostBlockState, action: TAction, state: CostEditorState, addionalParam?: TParam) => CostBlockState)
) => 
        (state: CostEditorState, action: Action<string>, addionalParam?: TParam) => {
            const costBlockAction = <TAction>action;
            const { applicationId, costBlockId } = costBlockAction;

            return mapCostBlock(
                state, 
                applicationId, 
                costBlockId,  
                costBlock => changeFn(costBlock, costBlockAction, state, addionalParam) 
            );
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
            inputLevels: {
                ...costElement.inputLevels,
                list: mapIf(
                    costElement.inputLevels.list, 
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

const selectCostElement = buildCostBlockChanger<CostElementAction>(
    (costBlock, { costElementId }) => (<CostBlockState>{
        ...costBlock, 
        costElements: {
            ...costBlock.costElements,
            selectedItemId: costElementId
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
        inputLevels: {
            ...costElement.inputLevels,
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
            selectedCostElement.inputLevels.list.find(item => item.inputLevelId === selectedCostElement.inputLevels.selectedItemId);

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

const selectCostBlock: Reducer<CostEditorState, CostBlockAction> = (state, { applicationId, costBlockId }) => mapApplication(
    state, 
    applicationId,
    app => (<ApplicationState>{
        ...app,
        costBlocks: {
            ...app.costBlocks,
            selectedItemId: costBlockId
        }
    })
)

export const costEditorReducer: Reducer<CostEditorState, Action<string>> = (state = defaultState(), action) => {
    switch(action.type) {
        case APP_LOAD_DATA:
            return initByMeta(state, <LoadingAppDataAction>action);

        case COST_EDITOR_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action)

        case COST_EDITOR_SELECT_COST_BLOCK:
            return selectCostBlock(state, <CostBlockAction>action);

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


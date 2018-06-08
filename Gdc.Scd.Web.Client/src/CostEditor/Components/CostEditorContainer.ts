import { Filter, CostBlockState } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, CostEditorState } from "../States/CostEditorStates";
import { CostBlockTab, CostEditorProps, CostEditorActions, CostEditorView } from "./CostEditorView";
import { connect } from "react-redux";
import { PageCommonState } from "../../Layout/States/PageStates";
import { init, 
    selectApplicationLosseDataCheck, 
    selectScopeLosseDataCheck, 
    selectCostBlock, 
    loseChanges, 
    hideDataLoseWarning 
} from "../Actions/CostEditorActions";
import { 
    selectCountryWithReloading, 
    getFilterItemsByCustomElementSelection, 
    loadEditItemsByContext, 
    getFilterItemsByInputLevelSelection, 
    changeSelectionCostElementFilter, 
    changeSelectionInputLevelFilter, 
    resetCostElementFilter, 
    resetInputLevelFilter, 
    clearEditItems, 
    editItem, 
    saveEditItemsToServer, 
    applyFiltersWithReloading 
} 
from "../Actions/CostBlockActions";

const isSetContainsAllCheckedItems = (set: Set<string>, filterObj: Filter) => {
    let result = true;

    if (filterObj && filterObj.filter) {
        const checkedItems = filterObj.filter.filter(item => item.isChecked);

        result = checkedItems.length == set.size && checkedItems.every(item => set.has(item.id))
    } 

    return result
}

const costBlockTabListMap = (
    costBlock: CostBlockState, 
    countries: NamedId[], 
    costBlockMeta: CostBlockMeta,
    inputLevel:  Map<string, NamedId> 
): CostBlockTab => {
    const { edit } = costBlock;

    const selectedCostElement = 
        costBlock.costElement.list.find(
            item => item.costElementId === costBlock.costElement.selectedItemId);

    const selectedCostElementMeta = 
        costBlockMeta.costElements.find(
            item => item.id === costBlock.costElement.selectedItemId);

    const selectedInputLevelMeta = inputLevel.get(costBlock.inputLevel.selectedItemId);
    const selectedInputLevel = 
        costBlock.inputLevel.list && 
        costBlock.inputLevel.list.find(
            item => item.inputLevelId === costBlock.inputLevel.selectedItemId)
    
    const isEnableEditButtons = edit.editedItems && edit.editedItems.length > 0;
    const isEnableList = !edit.editedItems || edit.editedItems.length == 0;

    return {
        id: costBlock.costBlockId,
        name: costBlockMeta.name,
        costBlock: {
            country: {
                selectedItemId: costBlock.selectedCountryId,
                list: countries
            },
            costElement: {
                selectList: {
                    selectedItemId: costBlock.costElement.selectedItemId,
                    list: costBlockMeta.costElements.filter(
                        costElement => costBlock.visibleCostElementIds.includes(costElement.id))
                },
                filter: selectedCostElement && selectedCostElement.filter,
                filterName: selectedCostElementMeta && 
                            selectedCostElementMeta.dependency && 
                            selectedCostElementMeta.dependency .name,
                description: selectedCostElementMeta && selectedCostElementMeta.description,
                isVisibleFilter: selectedCostElement && selectedCostElement.filter && selectedCostElement.filter.length > 0,
                isEnableList: isEnableList
            },
            inputLevel: {
                selectList: {
                    selectedItemId: costBlock.inputLevel.selectedItemId,
                    list: Array.from(inputLevel.values())
                },
                filter: selectedInputLevel && selectedInputLevel.filter,
                filterName: selectedInputLevelMeta && selectedInputLevelMeta.name,
                isVisibleFilter: selectedInputLevel && selectedInputLevel.filter && selectedInputLevel.filter.length > 0,
                isEnableList: isEnableList
            },
            edit: {
                nameColumnTitle: selectedInputLevelMeta && selectedInputLevelMeta.name,
                valueColumnTitle: selectedCostElementMeta && selectedCostElementMeta.name,
                isVisible: costBlock.costElement.selectedItemId != null && 
                           costBlock.inputLevel.selectedItemId !=null,
                items: edit.originalItems && edit.originalItems.map(originalItem => ({
                    ...edit.editedItems.find(editedItem => editedItem.id === originalItem.id) || originalItem
                })),
                isEnableClear: isEnableEditButtons,
                isEnableSave: isEnableEditButtons,
                isEnableApplyFilters: !isSetContainsAllCheckedItems(edit.appliedFilter.costElementsItemIds, selectedCostElement) ||
                                      !isSetContainsAllCheckedItems(edit.appliedFilter.inputLevelItemIds, selectedInputLevel)
            }
        }
    }
}

export const CostEditorContainer = connect<CostEditorProps,CostEditorActions,{},PageCommonState<CostEditorState>>(
    state => {
        const { 
            applications, 
            selectedApplicationId,
            scopes,  
            selectedScopeId,
            visibleCostBlockIds,
            selectedCostBlockId,
            costBlocks,
            countries: countryMap,
            costBlockMetas,
            inputLevels,
            dataLossInfo,
        } = state.page.data;

        const countryArray = countryMap && Array.from(countryMap.values()) || [];

        return {
            application: {
                selectedItemId: selectedApplicationId,
                list: applications && Array.from(applications.values())
            },
            scope: {
                selectedItemId: selectedScopeId,
                list: scopes && Array.from(scopes.values())
            },
            isDataLossWarningDisplayed: dataLossInfo.isWarningDisplayed,
            costBlocks: {
                selectedItemId: selectedCostBlockId,
                list: costBlocks && 
                      costBlocks.filter(costBlock => visibleCostBlockIds.includes(costBlock.costBlockId))
                                      .map(costBlock => 
                                        costBlockTabListMap(
                                            costBlock, 
                                            countryArray, 
                                            costBlockMetas.get(costBlock.costBlockId), 
                                            inputLevels))
            }
        } as CostEditorProps;
    },
    dispatch => ({
        onInit: () => dispatch(init()),
        onApplicationSelected: applicationId => dispatch(selectApplicationLosseDataCheck(applicationId)),
        onScopeSelected: scopeId => dispatch(selectScopeLosseDataCheck(scopeId)),
        onCostBlockSelected: costBlockId => dispatch(selectCostBlock(costBlockId)),
        onLoseChanges: () => dispatch(loseChanges()),
        onCancelDataLose: () => dispatch(hideDataLoseWarning()),
        tabActions: {
            onCountrySelected: (countryId, costBlockId) => {
                dispatch(selectCountryWithReloading(costBlockId, countryId));
            },
            onCostElementSelected: (costBlockId, costElementId) => {
                dispatch(getFilterItemsByCustomElementSelection(costBlockId, costElementId));
                dispatch(loadEditItemsByContext());
            },
            onInputLevelSelected: (costBlockId, inputLevelId) => {
                dispatch(getFilterItemsByInputLevelSelection(costBlockId, inputLevelId));
                dispatch(loadEditItemsByContext());
            },
            onCostElementFilterSelectionChanged: (costBlockId, costElementId, filterItemId, isSelected) => {
                dispatch(changeSelectionCostElementFilter(costBlockId, costElementId, filterItemId, isSelected));
            },
            onInputLevelFilterSelectionChanged: (costBlockId, inputLevelId, filterItemId, isSelected) => {
                dispatch(changeSelectionInputLevelFilter(costBlockId, inputLevelId, filterItemId, isSelected));
            },
            onCostElementFilterReseted: (costBlockId, costElementId) => {
                dispatch(resetCostElementFilter(costBlockId, costElementId));
                dispatch(loadEditItemsByContext());
            },
            onInputLevelFilterReseted: (costBlockId, inputLevelId) => {
                dispatch(resetInputLevelFilter(costBlockId, inputLevelId))
                dispatch(loadEditItemsByContext());
            },
            onEditItemsCleared: costBlockId => dispatch(clearEditItems(costBlockId)),
            onItemEdited: (costBlockId, item) => dispatch(editItem(costBlockId, item)),
            onEditItemsSaving: costBlockId => dispatch(saveEditItemsToServer(costBlockId)),
            onApplyFilters: costBlockId => dispatch(applyFiltersWithReloading(costBlockId))
        }
    })
)(CostEditorView);
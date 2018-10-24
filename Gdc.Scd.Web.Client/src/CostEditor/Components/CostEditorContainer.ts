import { Filter, CostBlockState, InputLevelState, CheckItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { CostEditorState } from "../States/CostEditorStates";
import { CostBlockTab, CostEditorProps, CostEditorActions, CostEditorView } from "./CostEditorView";
import { connect } from "react-redux";
import { 
    selectApplicationLosseDataCheck, 
    selectCostBlock, 
    loseChanges, 
    hideDataLoseWarning 
} from "../Actions/CostEditorActions";
import { 
    selectRegionWithReloading, 
    getDataByCostElementSelection, 
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
import { SelectListFilter, RegionProps, CostElementProps } from "./CostBlocksView";
import { EditGridToolProps } from "./EditGridTool";
import { CommonState } from "../../Layout/States/AppStates";
import { InputLevelMeta, CostBlockMeta, FieldType } from "../../Common/States/CostMetaStates";
import { filterCostEditorItems } from "../Helpers/CostEditorHelpers";
import { findMeta } from "../../Common/Helpers/MetaHelper";

const isSetContainsAllCheckedItems = (set: Set<string>, filterObj: Filter) => {
    let result = true;

    if (filterObj && filterObj.filter) {
        const checkedItems = filterObj.filter.filter(item => item.isChecked);

        result = checkedItems.length == set.size && checkedItems.every(item => set.has(item.id))
    } 

    return result
}

const buildInputLevel = (
    costBlock: CostBlockState, 
    selectedInputLevel: InputLevelState,
    selectedInputLevelMeta: InputLevelMeta,
    inputLevelMetas: InputLevelMeta[],
    isEnableList: boolean
 ) => {
    let filter: CheckItem[];
    let filterName: string;
    let selectedInputLevelId: string = null;
    let isVisibleFilter: boolean;
    
    if (selectedInputLevel) {
        isVisibleFilter = selectedInputLevel.filter && selectedInputLevel.filter.length > 0;
        if (isVisibleFilter) {
            const prevLevelNumber = selectedInputLevelMeta.levelNumer - 1;
            const prevInputLevelMeta = inputLevelMetas.find(item => item.levelNumer === prevLevelNumber);
            
            filterName = prevInputLevelMeta.name;
            filter = selectedInputLevel.filter;
        }

        selectedInputLevelId = selectedInputLevel.inputLevelId;
    }

    return <SelectListFilter>{
        id: `${costBlock.costBlockId}_${costBlock.costElements.selectedItemId}`,
        filter,
        filterName,
        isVisibleFilter,
        isEnableList: isEnableList,
        selectList: {
            selectedItemId: selectedInputLevelId,
            list: inputLevelMetas
        }
    }
};

const costBlockTabMap = (
    costBlock: CostBlockState, 
    costBlockMeta: CostBlockMeta
): CostBlockTab => {
    const { edit } = costBlock;

    let regionProps: RegionProps;
    let inputLevel: SelectListFilter;
    let editProps: EditGridToolProps;
    let selectedInputLevelMeta: InputLevelMeta;
    let selectedInputLevel: InputLevelState;
    
    const isEnableEditButtons = edit.editedItems && edit.editedItems.length > 0;
    const isEnableList = !edit.editedItems || edit.editedItems.length == 0;

    const costElementProps = <CostElementProps>{
        id: costBlock.costBlockId,
        selectList: {
            selectedItemId: costBlock.costElements.selectedItemId,
            // // // list: costBlockMeta.costElements.filter(
            // // //     costElement => costBlock.visibleCostElementIds.includes(costElement.id))
            // list: costBlockMeta.costElements.filter(
            //     costElement => 
            //         costElement.inputType === InputType.Manually || 
            //         costElement.inputType === InputType.ManuallyAutomaticly
            // )
            list: filterCostEditorItems(costBlockMeta.costElements)
        },
        isEnableList,
        isVisibleFilter: false,
    }

    if (costBlock.costElements.selectedItemId != null) {
        const selectedCostElement = 
            costBlock.costElements.list.find(
                item => item.costElementId === costBlock.costElements.selectedItemId);

        const selectedCostElementMeta = 
            costBlockMeta.costElements.find(
                item => item.id === costBlock.costElements.selectedItemId);

        if (selectedCostElement.inputLevel.selectedItemId != null) {
            selectedInputLevelMeta = 
                selectedCostElementMeta.inputLevels.find(inputLevel => inputLevel.id === selectedCostElement.inputLevel.selectedItemId);

            selectedInputLevel = 
                selectedCostElement.inputLevel.list.find(item => item.inputLevelId === selectedCostElement.inputLevel.selectedItemId);

            editProps = {
                editGrid: {
                    nameColumnTitle: selectedInputLevelMeta.name,
                    valueColumn: {
                        title: selectedCostElementMeta.name,
                        type: selectedCostElementMeta.typeOptions ? selectedCostElementMeta.typeOptions.Type : FieldType.Double,
                        selectedItems: selectedCostElement.referenceValues
                    },
                    items: edit.originalItems && edit.originalItems.map(originalItem => ({
                        ...edit.editedItems.find(editedItem => editedItem.id === originalItem.id) || originalItem
                    }))
                },
                costBlockId: costBlock.costBlockId,
                qualityGateErrors: costBlock.edit.saveErrors,
                isEnableClear: isEnableEditButtons,
                isEnableSave: isEnableEditButtons,
                isEnableApplyFilters: !isSetContainsAllCheckedItems(edit.appliedFilter.costElementsItemIds, selectedCostElement) ||
                                      !isSetContainsAllCheckedItems(edit.appliedFilter.inputLevelItemIds, selectedInputLevel) 
            }
        }

        inputLevel = buildInputLevel(
            costBlock, 
            selectedInputLevel, 
            selectedInputLevelMeta, 
            selectedCostElementMeta.inputLevels, 
            isEnableList);

        if (selectedCostElementMeta.regionInput){
            regionProps = {
                name: selectedCostElementMeta.regionInput.name,
                selectedList: selectedCostElement.region
            }
        }

        costElementProps.filter = selectedCostElement.filter;
        costElementProps.filterName = selectedCostElementMeta.dependency && selectedCostElementMeta.dependency.name;
        costElementProps.description = selectedCostElementMeta.description;
        costElementProps.isVisibleFilter = selectedCostElement.filter && selectedCostElement.filter.length > 0;
    }

    return {
        id: costBlock.costBlockId,
        name: costBlockMeta.name,
        costBlock: {
            region: regionProps,
            costElement: costElementProps,
            inputLevel: inputLevel,
            edit: editProps
        }
    }
}

export const CostEditorContainer = connect<CostEditorProps,CostEditorActions,{},CommonState>(
    state => {
        const { 
            //applications, 
            selectedApplicationId,
            //visibleCostBlockIds,
            selectedCostBlockId,
            costBlocks,
            //costBlockMetas,
            dataLossInfo,
        } = state.pages.costEditor;

        const { applications: applicationMets, costBlocks: costBlockMetas } = state.app.appMetaData;

        return {
            application: {
                selectedItemId: selectedApplicationId,
                list: <NamedId[]>filterCostEditorItems(applicationMets)
                //list: applications && Array.from(applications.values())
            },
            isDataLossWarningDisplayed: dataLossInfo.isWarningDisplayed,
            costBlocks: {
                selectedItemId: selectedCostBlockId,
                // list: costBlocks && 
                //     //   //costBlocks.filter(costBlock => visibleCostBlockIds.includes(costBlock.costBlockId))
                //     //   costBlocks.filter(costBlock => findMeta(costBlockMetas, costBlock.costBlockId).applicationIds.includes(selectedApplicationId))
                //     //             .map(costBlock => 
                //     //                 costBlockTabMap(
                //     //                     costBlock, 
                //     //                     costBlockMetas.get(costBlock.costBlockId)))
                list: costBlocks &&
                      costBlocks.map(costBlock => ({ state: costBlock, meta: findMeta(costBlockMetas, costBlock.costBlockId) }))
                                .filter(costBlockInfo => costBlockInfo.meta.applicationIds.includes(selectedApplicationId))
                                .map(costBlockInfo => costBlockTabMap(costBlockInfo.state, costBlockInfo.meta))                  
            }
        } as CostEditorProps;
    },
    dispatch => ({
        onApplicationSelected: applicationId => dispatch(selectApplicationLosseDataCheck(applicationId)),
        onCostBlockSelected: costBlockId => dispatch(selectCostBlock(costBlockId)),
        onLoseChanges: () => dispatch(loseChanges()),
        onCancelDataLose: () => dispatch(hideDataLoseWarning()),
        tabActions: {
            onRegionSelected: (regionId, costBlockId) => {
                dispatch(selectRegionWithReloading(costBlockId, regionId));
            },
            onCostElementSelected: (costBlockId, costElementId) => {
                dispatch(getDataByCostElementSelection(costBlockId, costElementId));
                dispatch(loadEditItemsByContext());
            },
            onInputLevelSelected: (costBlockId, costElementId, inputLevelId) => {
                dispatch(getFilterItemsByInputLevelSelection(costBlockId, costElementId, inputLevelId));
                dispatch(loadEditItemsByContext());
            },
            onCostElementFilterSelectionChanged: (costBlockId, costElementId, filterItemId, isSelected) => {
                dispatch(changeSelectionCostElementFilter(costBlockId, costElementId, filterItemId, isSelected));
            },
            onInputLevelFilterSelectionChanged: (costBlockId, costElementId, inputLevelId, filterItemId, isSelected) => {
                dispatch(changeSelectionInputLevelFilter(costBlockId, costElementId, inputLevelId, filterItemId, isSelected));
            },
            onCostElementFilterReseted: (costBlockId, costElementId) => {
                dispatch(resetCostElementFilter(costBlockId, costElementId));
            },
            onInputLevelFilterReseted: (costBlockId, costElementId, inputLevelId) => {
                dispatch(resetInputLevelFilter(costBlockId, costElementId, inputLevelId))
            },
            onEditItemsCleared: costBlockId => dispatch(clearEditItems(costBlockId)),
            onItemEdited: (costBlockId, item) => dispatch(editItem(costBlockId, item)),
            onEditItemsSaving: (costBlockId, forApproval) => dispatch(saveEditItemsToServer(costBlockId, { isApproving: forApproval })),
            onApplyFilters: costBlockId => dispatch(applyFiltersWithReloading(costBlockId))
        }
    })
)(CostEditorView);
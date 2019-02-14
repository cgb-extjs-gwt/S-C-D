import { asyncAction } from "../../Common/Actions/AsyncAction";
import { getCostBlock, findMeta, getCostElementByAppMeta, getCostElement } from "../../Common/Helpers/MetaHelper";
import { selectCostElement, selectInputLevel, loadInputLevelFilter, editItemsUrlChanged, selectRegion, applyFilters, saveEditItems, loadCostElementData, loadDependencyFilter } from "./CostBlockActions";
import { buildCostEditorContext, findCostElementByState, findInputeLevelByState, findInputLevel, findCostBlockByState, findApplication, findCostBlock, findCostElement } from "../Helpers/CostEditorHelpers";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import * as service from "../Services/CostEditorServices";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";
import { getDependencyItems } from "../../Common/Services/CostBlockService";
import { CostElementId } from "../../Approval/States/ApprovalState";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";
import { Dispatch } from "redux";

export const loadDataByCostElementSelection = (applicationId: string, costBlockId: string, costElementId: string) =>
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const { app: { appMetaData } } = getState();
            const costBlockMeta = getCostBlock(appMetaData, costBlockId);   

            dispatch(selectCostElement(applicationId, costElementId, costBlockMeta));

            const state = getState();
            const { pages: { costEditor } } = state;
            const context = buildCostEditorContext(costEditor);
            const costElement = findCostElementByState(costEditor, applicationId, costBlockId, costElementId);

            const promises = [];
            const costElementMeta = getCostElement(costBlockMeta, costElementId);
            
            if (!costElementMeta.regionInput) {
                promises.push(loadDependencyFilterFromServer(dispatch, state));
            }

            if (!costElement.isDataLoaded) {
                promises.push(
                    service.getCostElementData(context).then(data => { 
                        dispatch(loadCostElementData(applicationId, costElementId, costBlockMeta, data));
                    })
                )
            }

            Promise.all(promises).then(() => {
                dispatch(loadEditItemsByContext());
            });

            if (costElement.inputLevels.selectedItemId == null) {
                dispatch(
                    loadDataByInputLevelSelection(
                        applicationId,
                        costBlockId, 
                        costElementId, 
                        costElement.
                        inputLevels.list[0].inputLevelId));
            }
        }
    )

export const loadDataByInputLevelSelection = (applicationId: string, costBlockId: string, costElementId: string, inputLevelId: string) =>
    asyncAction<CommonState>(
        (dispatch, getState) => {
            dispatch(selectInputLevel(applicationId, costBlockId, costElementId, inputLevelId));

            const state = getState();
            const costEditor = state.pages.costEditor;
            const costBlockMeta = findMeta(state.app.appMetaData.costBlocks, costBlockId);
            const costElementMeta = findMeta(costBlockMeta.costElements, costElementId);
            const inputLevelMeta = findMeta(costElementMeta.inputLevels, inputLevelId);

            if (inputLevelMeta.hasFilter) {
                const inputLevel = findInputeLevelByState(costEditor, applicationId, costBlockId, costElementId, inputLevelId)
                
                if (!inputLevel || !inputLevel.filter)
                {
                    const context = buildCostEditorContext(costEditor);
                    
                    handleRequest(
                        service.getLevelInputFilterItems(context).then(filterItems => {
                            dispatch(loadInputLevelFilter(applicationId, costBlockId, costElementId, inputLevelId, filterItems));
                            dispatch(loadEditItemsByContext());
                        })
                    )
                } else {
                    dispatch(loadEditItemsByContext());
                }
            } else {
                dispatch(loadEditItemsByContext());
            }
        }
    )

export const loadEditItemsByContext = () => 
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const { app: { appMetaData }, pages: { costEditor } } = getState();
            const context = buildCostEditorContext(costEditor);
            const costBlockMeta = findMeta(appMetaData.costBlocks, context.costBlockId);

            if (context.costElementId != null && context.inputLevelId != null) {
                const costElementMeta = findMeta(costBlockMeta.costElements, context.costElementId);

                let isDispatching: boolean = null;

                if (!costElementMeta.regionInput || context.regionInputId) {
                    const costElementState = findCostElementByState(costEditor);

                    isDispatching = costElementMeta.dependency == null || costElementState.filter != null;

                    if (isDispatching !== false) {
                        const inputLevelMeta = findMeta(costElementMeta.inputLevels, context.inputLevelId);

                        if (inputLevelMeta.hasFilter) {
                            const inputLevelState = findInputLevel(costElementState.inputLevels);

                            isDispatching = inputLevelState.filter != null;
                        } else {
                            isDispatching = true;
                        }
                    }
                }

                const newEditItemsUrl = service.buildGetEditItemsUrl(context);
                const { edit } = findCostBlockByState(costEditor)

                if (isDispatching && edit.editItemsUrl != newEditItemsUrl) {
                    dispatch(
                        editItemsUrlChanged(context.applicationId, context.costBlockId, newEditItemsUrl)
                    )
                }
            }
        }
    )

export const saveEditItemsToServer = (applicationId: string, costBlockId: string, approvalOption: ApprovalOption) => 
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const state = getState().pages.costEditor
            const costBlock = findCostBlockByState(state, applicationId, costBlockId);
            const context = buildCostEditorContext(state);

            handleRequest(
                service.saveEditItems(costBlock.edit.editedItems, context, approvalOption)
                       .then(
                            qualityGateResult => dispatch(saveEditItems(applicationId, costBlockId, qualityGateResult))
                       )
            )
        }
    )

export const selectRegionWithReloading = (applicationId: string, costBlockId: string, regionId: number) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const { pages: { costEditor } } = getState();
        const { costElements } = findCostBlockByState(costEditor, applicationId, costBlockId);
        const costElementId = costElements.selectedItemId;

        dispatch(selectRegion(applicationId, costBlockId, costElementId, regionId));

        loadDependencyFilterFromServer(dispatch, getState()).then(() => {
            dispatch(loadEditItemsByContext());
        })
    }
)

export const applyFiltersWithReloading = (applicationId: string, costBlockId: string) => asyncAction<CommonState>(
     dispatch => {
        dispatch(applyFilters(applicationId, costBlockId));
        dispatch(loadEditItemsByContext());
    }
)

const loadDependencyFilterFromServer = (dispatch: Dispatch, state: CommonState) => new Promise<any>(
    resolve => {
        const { pages: { costEditor }, app: { appMetaData } } = state;
        const application = findApplication(costEditor);
        const costBlock = findCostBlock(application.costBlocks);

        if (costBlock.costElements.selectedItemId) {
            const costElementMeta = getCostElementByAppMeta(appMetaData, costBlock.costBlockId, costBlock.costElements.selectedItemId);

            if (costElementMeta.dependency) {
                const costElement = findCostElement(costBlock.costElements);
                const costElementId: CostElementIdentifier = {
                    applicationId: application.id,
                    costBlockId: costBlock.costBlockId,
                    costElementId: costBlock.costElements.selectedItemId
                }
    
                handleRequest(
                    getDependencyItems(costElementId, costElement.region && costElement.region.selectedItemId).then(dependencies => {
                        dispatch(loadDependencyFilter(application.id, costBlock.costBlockId, costBlock.costElements.selectedItemId, dependencies));
                        resolve();
                    })
                )
            } else {
                resolve();
            }
        } else {
            resolve();
        }
    }
)

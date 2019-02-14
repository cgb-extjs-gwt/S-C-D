import { connectAdvanced, Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { CostImportViewProps, ResultImportItem, CostImportView } from "./CostImportView";
import { NamedId, SelectListAdvanced } from "../../Common/States/CommonStates";
import { CostImportState } from "../States/CostImportState";
import { CostMetaData, CostElementMeta } from "../../Common/States/CostMetaStates";
import { getCostBlock, getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { selectApplication, selectCostBlock, selectCostElement, selectDependencyItem, selectFile, selectRegion, loadFileData, loadRegions, loadDependencyItems } from "../Actions/CostImportActions";
import { importExcel, loadDependencyItemsFromServer, loadRegionsFromServer } from "../Actions/CostImportAsyncAtions";
import * as CostBlockService from "../../Common/Services/CostBlockService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { getBase64Data } from "../../Common/Helpers/FileHelper";

const buildProps = (() => {
    let oldProps: CostImportViewProps = {
        applications: {
            list: null,
            selectedItemId: null
        },
        costBlocks: {
            list: null,
            selectedItemId: null
        },
        costElements: {
            list: null,
            selectedItemId: null
        },
        dependencyItems: {
            list: null,
            selectedItemId: null
        },
        regions: {
            list: null,
            selectedItemId: null
        },
        isImportButtonEnabled: false,
        isVisibleDependencyItems: false,
        isVisibleRegions: false,
        resultImport: null
    };
    let oldAppMetaData: CostMetaData;
    let oldCostImport: CostImportState

    return (dispatch: Dispatch, { app: { appMetaData }, pages: { costImport } }: CommonState) => {
        let props: CostImportViewProps;

        if (oldCostImport == costImport && oldAppMetaData == appMetaData) {
            props = oldProps;
        } else {
            let isVisibleDependencyItems = false;
            let isVisibleRegions = false;

            const costElement = getCostElementByAppMeta(appMetaData, costImport.costBlockId, costImport.costElementId);

            if (costElement) {
                isVisibleDependencyItems = !!costElement.dependency;
                isVisibleRegions = !!costElement.regionInput;
            }

            oldProps = props = {
                applications: {
                    list: getApplicationItems(),
                    selectedItemId: costImport.applicationId,
                    onItemSelected: applicationId => dispatch(selectApplication(applicationId))
                },
                costBlocks: {
                    list: getCostBlockItems(),
                    selectedItemId: costImport.costBlockId,
                    onItemSelected: costBlockId => dispatch(selectCostBlock(costBlockId))
                },
                costElements: {
                    list: getCostElementItems(),
                    selectedItemId: costImport.costElementId,
                    onItemSelected: onCostElementSelected
                },
                dependencyItems: {
                    list: costImport.dependencyItems.list,
                    selectedItemId: costImport.dependencyItems.selectedItemId,
                    onItemSelected: dependencyItemId => dispatch(selectDependencyItem(dependencyItemId))
                },
                regions: {
                    list: costImport.regions.list,
                    selectedItemId: costImport.regions.selectedItemId,
                    onItemSelected: onRegionSelected
                },
                isImportButtonEnabled: isImportButtonEnabled(costElement),
                resultImport: getImportResult(),
                isVisibleDependencyItems,
                isVisibleRegions,
                onFileSelect: fileName => dispatch(selectFile(fileName)),
                onUnmount: () => dispatch(selectFile(null)),
                onImport
            }

            oldAppMetaData = appMetaData;
            oldCostImport = costImport;
        }

        return props;

        function getApplicationItems() {
            return oldAppMetaData == appMetaData 
                ? oldProps.applications.list
                : appMetaData.applications.filter(application => application.usingInfo.isUsingCostImport) ;
        }

        function getCostBlockItems() {
            let costBlockItems: NamedId[];

            if (oldAppMetaData == appMetaData && oldProps.applications.selectedItemId == costImport.applicationId) {
                costBlockItems = oldProps.costBlocks.list;
            } else {
                costBlockItems = appMetaData.costBlocks.filter(
                    costBlock => 
                        costBlock.usingInfo.isUsingCostImport &&
                        costBlock.applicationIds.includes(costImport.applicationId)
                )
            }

            return costBlockItems
        }

        function getCostElementItems() {
            let costElementItems: NamedId[];

            if (oldAppMetaData == appMetaData && oldProps.costBlocks.selectedItemId == costImport.costBlockId) {
                costElementItems = oldProps.costElements.list;
            } else {
                const costBlock = getCostBlock(appMetaData, costImport.costBlockId);

                if (costBlock) {
                    costElementItems = costBlock.costElements.filter(costElement => costElement.usingInfo.isUsingCostImport);
                }
            }

            return costElementItems;
        }

        function isImportButtonEnabled(costElement: CostElementMeta) {
            let result = !!(
                costImport.applicationId && 
                costImport.costBlockId &&
                costImport.costElementId &&
                costImport.file.name
            );

            if (result && costElement.dependency) {
                result = costImport.dependencyItems.selectedItemId != null;
            }

            if (result && costElement.regionInput) {
                result = costImport.regions.selectedItemId != null;
            }

            return result;
        }

        function getImportResult() {
            return costImport.status.map(error => (<ResultImportItem>{
                info: error
            }));
        }

        function onCostElementSelected(costElementId: string) {
            dispatch(selectCostElement(costElementId));

            const { applicationId, costBlockId } = costImport;
            const costElement = getCostElementByAppMeta(appMetaData, costBlockId, costElementId);

            if (costElement) {
                if (costElement.regionInput) {
                    dispatch(loadRegionsFromServer(applicationId, costBlockId, costElementId));
                } else if (costElement.dependency) {
                    dispatch(loadDependencyItemsFromServer(applicationId, costBlockId, costElementId));
                }
            }
        }

        function onRegionSelected(regionId: number) {
            dispatch(selectRegion(regionId));

            const { applicationId, costBlockId, costElementId } = costImport;

            dispatch(loadDependencyItemsFromServer(applicationId, costBlockId, costElementId, regionId));
        }

        function onImport(file) {
            getBase64Data(file).then(fileData => {
                dispatch(loadFileData(fileData));
                dispatch(importExcel({ isApproving: true }));
            });
        }
    }
})()

export const CostImportContainer = connectAdvanced<CommonState, CostImportViewProps, {}>(
    dispatch => (state, ownProps) => buildProps(dispatch, state)
)(CostImportView)
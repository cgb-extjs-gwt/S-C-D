import { connectAdvanced, Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { CostImportViewProps, ResultImportItem, CostImportView } from "./CostImportView";
import { NamedId, SelectListAdvanced } from "../../Common/States/CommonStates";
import { CostImportState } from "../States/CostImportState";
import { CostMetaData, CostElementMeta } from "../../Common/States/CostMetaStates";
import { getCostBlock, getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { selectApplication, selectCostBlock, selectCostElement, selectDependencyItem, loadCostElementData, selectFile, loadImportStatus, selectRegion } from "../Actions/CostImportActions";
import * as CostBlockService from "../../Common/Services/CostBlockService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

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
                    list: getDependencyItems(),
                    selectedItemId: costImport.dependencyItems.selectedItemId,
                    onItemSelected: dependencyItemId => dispatch(selectDependencyItem(dependencyItemId))
                },
                regions: {
                    list: getRegions(),
                    selectedItemId: costImport.regions.selectedItemId,
                    onItemSelected: regionId => dispatch(selectRegion(regionId))
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
            return oldAppMetaData == appMetaData ? oldAppMetaData.applications : appMetaData.applications;
        }

        function getCostBlockItems() {
            let costBlockItems: NamedId[];

            if (oldProps.applications.selectedItemId == costImport.applicationId) {
                costBlockItems = oldProps.costBlocks.list;
            } else {
                costBlockItems = appMetaData.costBlocks.filter(
                    costBlock => 
                        costBlock.isUsingCostImport &&
                        costBlock.applicationIds.includes(costImport.applicationId)
                )
            }

            return costBlockItems
        }

        function getCostElementItems() {
            let costElementItems: NamedId[];

            if (oldProps.costBlocks.selectedItemId == costImport.costBlockId) {
                costElementItems = oldProps.costElements.list;
            } else {
                const costBlock = getCostBlock(appMetaData, costImport.costBlockId);

                if (costBlock) {
                    costElementItems = costBlock.costElements.filter(costElement => costElement.isUsingCostImport);
                }
            }

            return costElementItems;
        }

        function getDependencyItems() {
            let dependencyItems: NamedId<number>[];

            if (oldProps.dependencyItems.list && 
                oldProps.dependencyItems.list.length > 0 &&
                oldProps.costElements.selectedItemId == costImport.costElementId) {
                dependencyItems = oldProps.dependencyItems.list;
            } else {
                dependencyItems = costImport.dependencyItems.list;
            }

            return dependencyItems;
        }

        function getRegions() {
            let regions: NamedId<number>[];

            if (oldProps.regions.list && 
                oldProps.regions.list.length > 0 &&
                oldProps.costElements.selectedItemId == costImport.costElementId) {
                regions = oldProps.regions.list;
            } else {
                regions = costImport.regions.list;
            }

            return regions;
        }

        function isImportButtonEnabled(costElement: CostElementMeta) {
            let result = !!(
                costImport.applicationId && 
                costImport.costBlockId &&
                costImport.costElementId &&
                costImport.fileName);

            if (result && costElement.dependency) {
                result = costImport.dependencyItems.selectedItemId != null;
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

            if (costElement && costElement.dependency) {
                handleRequest(
                    CostBlockService.getCostElementData({ applicationId, costBlockId, costElementId }).then(
                        costElementData => dispatch(loadCostElementData(costElementData))
                    )
                )
            }
        }

        function onImport(file) {
            const { applicationId, costBlockId, costElementId } = costImport;
            const dependencyItemId = costImport.dependencyItems.selectedItemId;
            const regionId = costImport.regions.selectedItemId;

            handleRequest(
                CostBlockService.importExcel({ applicationId, costBlockId, costElementId }, file, dependencyItemId, regionId).then(
                    ({ errors }) => {
                        const status = errors && errors.length > 0 
                            ? [ ...errors, 'Import completed' ]
                            : ['Import successfully completed']

                        dispatch(loadImportStatus(status))
                    },
                    () => dispatch(loadImportStatus(['Error during import']))
                )
            )
        }
    }
})()

export const CostImportContainer = connectAdvanced<CommonState, CostImportViewProps, {}>(
    dispatch => (state, ownProps) => buildProps(dispatch, state)
)(CostImportView)
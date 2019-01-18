import { connectAdvanced, Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { CostImportViewProps, ResultImportItem, CostImportView } from "./CostImportView";
import { NamedId, SelectListAdvanced } from "../../Common/States/CommonStates";
import { CostImportState } from "../States/CostImportState";
import { CostMetaData, CostElementMeta } from "../../Common/States/CostMetaStates";
import { getCostBlock, getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { selectApplication, selectCostBlock, selectCostElement, selectDependencyItem, loadDependencyItems, selectFile, loadImportStatus } from "../Actions/CostImportActions";
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
        isImportButtonEnabled: false,
        isVisibleDependencyItems: false,
        resultImport: null
    };
    let oldAppMetaData: CostMetaData;
    let oldCostImport: CostImportState
    let oldCostBlocks: NamedId[];
    let oldCostElements: NamedId[];
    let oldDependencies: NamedId<number>[];

    return (dispatch: Dispatch, { app: { appMetaData }, pages: { costImport } }: CommonState) => {
        let props: CostImportViewProps;

        if (oldCostImport == costImport && oldAppMetaData == appMetaData) {
            props = oldProps;
        } else {
            const costElement = getCostElementByAppMeta(appMetaData, costImport.costBlockId, costImport.costElementId);

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
                isImportButtonEnabled: isImportButtonEnabled(costElement),
                resultImport: getImportResult(),
                isVisibleDependencyItems: costElement ? !!costElement.dependency : false,
                onFileSelect: fileName => dispatch(selectFile(fileName)),
                onUnmount: () => dispatch(selectFile(null)),
                onImport
            }

            oldAppMetaData = appMetaData;
            oldCostImport = costImport;
            oldCostBlocks = props.costBlocks.list;
            oldCostElements = props.costElements.list;
            oldDependencies = props.dependencyItems.list;
        }

        return props;

        function getApplicationItems() {
            return oldAppMetaData == appMetaData ? oldAppMetaData.applications : appMetaData.applications;
        }

        function getCostBlockItems() {
            let costBlockItems: NamedId[];

            if (oldProps.applications.selectedItemId == costImport.applicationId) {
                costBlockItems = oldCostBlocks;
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
                costElementItems = oldCostElements;
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
                dependencyItems = oldDependencies;
            } else {
                dependencyItems = costImport.dependencyItems.list;
            }

            return dependencyItems;
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
                    CostBlockService.getDependencyItems({ applicationId, costBlockId, costElementId }).then(
                        dependencyItems => dispatch(loadDependencyItems(dependencyItems))
                    )
                )
            }
        }

        function onImport(file) {
            const { applicationId, costBlockId, costElementId, dependencyItems } = costImport;

            handleRequest(
                CostBlockService.importExcel({ applicationId, costBlockId, costElementId }, file, dependencyItems.selectedItemId).then(
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
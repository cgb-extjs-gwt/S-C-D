import * as React from 'react';
import { 
    Container, 
    FormPanel,
    ComboBoxField, 
    ContainerField, 
    RadioField, 
    Label,
    Panel,
    TabPanel
} from '@extjs/ext-react';
import FixedTabPanel from '../../Common/Components/FixedTabPanel';
import { connect } from 'react-redux';
import { PageCommonState, PageState, PAGE_STATE_KEY } from '../../Layout/States/PageStates';
import { CostEditorState, CostBlockMeta } from '../States/CostEditorStates';
import { getCostEditorDto } from '../Services/CostEditorServices';
import { CostBlockState, EditItem, CheckItem, Filter } from '../States/CostBlockStates';
import { selectCountry, 
    selectCostElement, 
    selectInputLevel, 
    getFilterItemsByCustomElementSelection, 
    getFilterItemsByInputLevelSelection, 
    reloadFilterBySelectedCountry, 
    changeSelectionCostElementFilter, 
    changeSelectionInputLevelFilter, 
    resetCostElementFilter, 
    resetInputLevelFilter, 
    loadEditItemsByContext, 
    clearEditItems, 
    editItem, 
    saveEditItemsToServer, 
    selectCountryWithReloading, 
    applyFiltersWithReloading 
} 
from '../Actions/CostBlockActions';
import { NamedId, SelectList } from '../../Common/States/CommonStates';
import { CostBlockProps, CostBlockView } from './CostBlocksView';

Ext.require('Ext.MessageBox');

export interface CostEditorActions {
    onInit?: () => void;
    onApplicationSelected?: (applicationId: string) => void;
    onScopeSelected?: (scopeId: string) => void;
    onCostBlockSelected?: (costBlockId: string) => void;
    onLoseChanges?: () => void
    onCancelDataLose?: () => void
    tabActions: {
        onCountrySelected?: (countryId: string, costBlockId: string) => void
        onCostElementSelected?: (costBlockId: string, costElementId: string) => void
        onInputLevelSelected?: (costBlockId: string, inputLevelId: string) => void
        onCostElementFilterSelectionChanged?: (
            costBlockId: string,
            costElementId: string, 
            filterItemId: string,
            isSelected: boolean) => void
        onInputLevelFilterSelectionChanged?: (
            costBlockId: string,
            inputLevelId: string, 
            filterItemId: string,
            isSelected: boolean) => void
        onCostElementFilterReseted?: (costBlockId: string, costElementId: string) => void
        onInputLevelFilterReseted?: (costBlockId: string, inputLevelId: string) => void
        onEditItemsCleared?: (costBlockId: string) => void
        onItemEdited?: (costBlockId: string, item: EditItem) => void
        onEditItemsSaving?: (costBlockId: string) => void
        onApplyFilters?: (costBlockId: string) => void
    }
}

export interface CostBlockTab extends NamedId {
    costBlock: CostBlockProps
}

export interface CostEditorProps extends CostEditorActions {
    application: SelectList<NamedId>
    scope: SelectList<NamedId>
    costBlocks: SelectList<CostBlockTab>
    isDataLossWarningDisplayed: boolean
}

export class CostEditorView extends React.Component<CostEditorProps> {
    private isShownDataLossWarning = false;

    constructor(props: CostEditorProps){
        super(props);
        props.onInit && props.onInit();
    }

    public componentDidUpdate() {
        const { isDataLossWarningDisplayed } = this.props;

        if (isDataLossWarningDisplayed && !this.isShownDataLossWarning) {
            this.showDataLossWarning();
            this.isShownDataLossWarning = true;
        }
    }

    public render() {
        const { application, scope, costBlocks } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    {this.applicationCombobox(application)}
                    {this.scopeCombobox(scope)}

                    {/* <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        { 
                            scope && 
                            scope.list && 
                            scope.list.map(item => this.scopeRadioFild(item, scope.selectedItemId))
                        }
                    </ContainerField> */}
                </FormPanel>

                <Container title="Cost Blocks:">
                    {
                        costBlocks && 
                        costBlocks.list &&
                        <FixedTabPanel 
                            tabBar={{
                                layout: { pack: 'left' }
                            }}
                            activeTab={
                                costBlocks.list.findIndex(costBlock => costBlock.id === costBlocks.selectedItemId)
                            }
                            onActiveItemChange={
                                (tabPanel, newValue, oldValue) => this.onActiveTabChange(tabPanel, newValue, oldValue)
                            }
                        >
                            {costBlocks.list.map(item => this.costBlockTab(item, costBlocks.selectedItemId))}
                        </FixedTabPanel>
                    }
                </Container>
            </Container>
        );
    }

    private onActiveTabChange = (tabPanel, newValue, oldValue) => {
        const costBlocks = this.props.costBlocks.list;

        if (costBlocks && costBlocks.length > 0) {
            const { onCostBlockSelected } = this.props;
            
            const activeTabIndex = tabPanel.getActiveItemIndex();
            const selectedCostBlockId = 
                activeTabIndex < costBlocks.length
                    ? costBlocks[activeTabIndex].id 
                    : costBlocks[0].id;

            onCostBlockSelected && onCostBlockSelected(selectedCostBlockId);
        }
    }

    private applicationCombobox(application: SelectList<NamedId>) {
        const { onApplicationSelected } = this.props;

        const applicatonStore = Ext.create('Ext.data.Store', {
            data: application && application.list
        });

        const selectedApplication = 
            applicatonStore.getData()
                           .findBy(item => (item.data as NamedId).id === application.selectedItemId);

        return (
            <ComboBoxField 
                label="Application"
                width="25%"
                displayField="name"
                valueField="id"
                queryMode="local"
                store={applicatonStore}
                selection={selectedApplication}
                onChange={(combobox, newValue, oldValue) => 
                    onApplicationSelected && onApplicationSelected(newValue)
                }
            />
        );
    }

    // private scopeRadioFild(scopeItem: NamedId, selectedScopeId: string) {
    //     const { onScopeSelected } = this.props;

    //     return (
    //         <RadioField 
    //             key={scopeItem.id} 
    //             itemId={scopeItem.id}
    //             boxLabel={scopeItem.name} 
    //             name="scope" 
    //             checked={scopeItem.id === selectedScopeId}
    //             onCheck={() => onScopeSelected && onScopeSelected(scopeItem.id) }
    //         />
    //     );
    // }

    private scopeCombobox(scopes: SelectList<NamedId>) {
        const { onScopeSelected } = this.props;

        const scopeStore = Ext.create('Ext.data.Store', {
            data: scopes && scopes.list
        });

        const selectedScope = 
            scopeStore.getData()
                      .findBy(item => (item.data as NamedId).id === scopes.selectedItemId);

        return (
            <ComboBoxField 
                label="Scope"
                width="25%"
                displayField="name"
                valueField="id"
                queryMode="local"
                store={scopeStore}
                selection={selectedScope}
                onChange={(combobox, newValue, oldValue) => 
                    onScopeSelected && onScopeSelected(newValue)
                }
            />
        );
    }

    private costBlockTab(costBlockTab: CostBlockTab, selectedCostBlockId: string) {
        const { 
            onCountrySelected, 
            onCostElementSelected, 
            onInputLevelSelected,
            onCostElementFilterSelectionChanged,
            onInputLevelFilterSelectionChanged,
            onCostElementFilterReseted,
            onInputLevelFilterReseted,
            onEditItemsCleared,
            onItemEdited,
            onEditItemsSaving,
            onApplyFilters
        } = this.props.tabActions;

        return (
            <Container key={costBlockTab.id} title={costBlockTab.name}>
                <CostBlockView 
                    {...costBlockTab.costBlock} 
                    onCountrySelected={
                        countryId => 
                            onCountrySelected && onCountrySelected(countryId, costBlockTab.id)
                    } 
                    onCostElementSelected={
                        costElementId => 
                            onCostElementSelected && onCostElementSelected(costBlockTab.id, costElementId)
                    }
                    onInputLevelSelected={
                        inputLevelId => 
                            onInputLevelSelected && onInputLevelSelected(costBlockTab.id, inputLevelId)
                    }
                    onCostElementFilterSelectionChanged={
                        (costElementId, filterItemId, isSelected) =>
                            onCostElementFilterSelectionChanged && 
                            onCostElementFilterSelectionChanged(costBlockTab.id, costElementId, filterItemId, isSelected)
                    }
                    onInputLevelFilterSelectionChanged={
                        (inputLevelId, filterItemId, isSelected) =>
                            onInputLevelFilterSelectionChanged && 
                            onInputLevelFilterSelectionChanged(costBlockTab.id, inputLevelId, filterItemId, isSelected)
                    }
                    onCostElementFilterReseted={
                        costElementId => 
                            onCostElementFilterReseted && 
                            onCostElementFilterReseted(costBlockTab.id, costElementId)
                    }
                    onInputLevelFilterReseted={
                        inputLevelId =>
                            onInputLevelFilterReseted && 
                            onInputLevelFilterReseted(costBlockTab.id, inputLevelId)
                    }
                    onEditItemsCleared={
                        () => onEditItemsCleared && onEditItemsCleared(costBlockTab.id)
                    }
                    onItemEdited={
                        item => onItemEdited && onItemEdited(costBlockTab.id, item)
                    }
                    onEditItemsSaving={
                        () => onEditItemsSaving && onEditItemsSaving(costBlockTab.id)
                    }
                    onApplyFilters={
                        () => onApplyFilters && onApplyFilters(costBlockTab.id)
                    }
                />
            </Container>
        );
    }

    private showDataLossWarning() {
        const { onLoseChanges, onCancelDataLose } = this.props;
        const me = this;

        const messageBox = Ext.Msg.confirm(
            'Warning', 
            'You have unsaved changes. If you continue, you will lose changes. Continue?',
            (buttonId: string) => {
                switch(buttonId) {
                    case 'yes':
                        onLoseChanges && onLoseChanges();
                        break;
                    case 'no':
                        onCancelDataLose && onCancelDataLose();
                        break;
                }

                me.isShownDataLossWarning = false
            }
        );
    }
}


import * as React from 'react';
import { Container, ComboBoxField, Panel, FormPanel, RadioField, ContainerField, Grid, Column, Toolbar, Button, Label, Dialog } from '@extjs/ext-react';
import { CostBlockState, EditItem, CheckItem } from '../States/CostBlockStates'
import { Filter } from './Filter';
import { SelectList, NamedId } from '../../Common/States/CommonStates';

Ext.require([
  'Ext.grid.plugin.CellEditing', 
  'Ext.MessageBox'
]);

export interface CostBlockActions {
  onRegionSelected?: (regionId: string) => void
  onCostElementSelected?: (costElementId: string) => void
  onCostElementFilterSelectionChanged?: (
    costElementId: string, 
    filterItemId: string,
    isSelected: boolean) => void
  onCostElementFilterReseted?: (costElementId: string) => void
  onInputLevelSelected?: (costElementId: string, inputLevelId: string) => void
  onInputLevelFilterSelectionChanged?: (
    costElementId: string,
    inputLevelId: string, 
    filterItemId: string,
    isSelected: boolean) => void
  onInputLevelFilterReseted?: (inputLevelId: string) => void
  onEditItemsCleared?: () => void
  onItemEdited?: (item: EditItem) => void
  onEditItemsSaving?: () => void
  onApplyFilters?: () => void
}

export interface SelectListFilter {
  id: string
  selectList: SelectList<NamedId>
  filter?: CheckItem[],
  filterName?: string,
  isVisibleFilter: boolean
  isEnableList: boolean
}

export interface CostElementProps extends SelectListFilter {
  description?: string
}

export interface RegionProps {
  selectedList: SelectList<NamedId>
  name: string
}

export interface EditProps {
  nameColumnTitle: string
  valueColumnTitle: string
  items: EditItem[]
  isEnableSave: boolean
  isEnableClear: boolean
  isEnableApplyFilters: boolean
}

export interface CostBlockProps {
  region: RegionProps
  costElement: CostElementProps
  inputLevel: SelectListFilter
  edit: EditProps
}

export class CostBlockView extends React.Component<CostBlockProps & CostBlockActions> {
  public render() {
    const { 
      costElement, 
      inputLevel,
      onCostElementSelected,
      onInputLevelSelected,
      onCostElementFilterSelectionChanged,
      onInputLevelFilterSelectionChanged,
      onCostElementFilterReseted,
      onInputLevelFilterReseted,
      edit
    } = this.props;

    return (
      <Container layout={{ type: 'hbox', align: 'stretch '}}>
        <Container flex={1} layout="vbox" shadow>
          <Container layout="hbox">
            <FormPanel flex={1}>
              {this.regionCombobox()}
              {
                this.radioFieldSet(
                  `${costElement.id}_costelements`, 
                  costElement.selectList, 
                  'Cost Elements', 
                  costElement.isEnableList,
                  costElement => onCostElementSelected && onCostElementSelected(costElement.id)
                )
              }
            </FormPanel>

            { 
              costElement.isVisibleFilter &&
              <Filter 
                title="Dependent from:" 
                valueColumnText={costElement.filterName}
                items={costElement.filter} 
                height="500"
                flex={1}
                onSelectionChanged={
                  (item: NamedId, isSelected: boolean) =>
                    onCostElementFilterSelectionChanged &&
                    onCostElementFilterSelectionChanged(
                      costElement.selectList.selectedItemId,
                      item.id,
                      isSelected
                    )
                }
                onReset={
                  () => 
                    onCostElementFilterReseted && 
                    onCostElementFilterReseted(costElement.selectList.selectedItemId)
                }
              />
            }
          </Container>

          <Panel title="Description" padding="10">
            {
              costElement.description != null &&
              <Label html={costElement.description}/>
            }
          </Panel>
        </Container>

        <Container flex={1} layout="vbox" padding="0px 0px 0px 5px">
          {
            inputLevel &&
            <Container layout="hbox">
              <FormPanel flex={1}>
                {
                  this.radioFieldSet(
                    `${inputLevel.id}_inputlevels`, 
                    inputLevel.selectList, 
                    'Input Level',
                    inputLevel.isEnableList,
                    inputLevel => onInputLevelSelected && onInputLevelSelected(costElement.selectList.selectedItemId, inputLevel.id)
                  )
                }
              </FormPanel>
              
              {
                inputLevel.isVisibleFilter &&
                <Filter 
                  valueColumnText={inputLevel.filterName}
                  items={inputLevel.filter} 
                  height="350"
                  flex={1}
                  onSelectionChanged={
                    (item: NamedId, isSelected: boolean) =>
                      onInputLevelFilterSelectionChanged &&
                      onInputLevelFilterSelectionChanged(
                        costElement.selectList.selectedItemId,
                        inputLevel.selectList.selectedItemId,
                        item.id,
                        isSelected
                      )
                  }
                  onReset={() => onInputLevelFilterReseted && onInputLevelFilterReseted(inputLevel.selectList.selectedItemId)}
                />
              }
            </Container>
          }
          

          {
            edit && 
            this.editGrid(edit.items, edit.nameColumnTitle, edit.valueColumnTitle)
          }
        </Container>
      </Container>
    );
  }

  private regionCombobox() {
    let result;

    const { region } = this.props;

    if (region) {
      const regionStore = Ext.create('Ext.data.Store', {
          data: region.selectedList ? region.selectedList.list : []
      });

      const selectedRegion = 
          regionStore.getData()
                      .findBy(item => (item.data as NamedId).id === region.selectedList.selectedItemId);

      result = (
          <ComboBoxField 
              label={region.name}
              //width="50%"
              displayField="name"
              valueField="id"
              queryMode="local"
              store={regionStore}
              selection={selectedRegion}
              onChange={(combobox, newValue, oldValue) => this.props.onRegionSelected(newValue)}
          />
      );
    }

    return result
  }

  private radioField(
    name: string, 
    item: NamedId, 
    selectedCostElementId: string,
    isEnabled: boolean,
    onSelected: (item: NamedId) => void
  ) {
    return (
      <RadioField 
          key={`${name}_${item.id}`} 
          boxLabel={item.name} 
          name={name} 
          checked={item.id === selectedCostElementId}
          disabled={!isEnabled}
          onCheck={
            radioField => {
              //HACK: onCheck event fired twice.
              if ((radioField as any).hasFocus) {
                onSelected(item);
              }

              return false;
            }
          }
      />
    );
  }

  private radioFieldSet(
    setName: string, 
    selectList: SelectList<NamedId>, 
    label: string, 
    isEnable: boolean,
    onSelected: (item: NamedId) => void
  ) {    
    return (
      <ContainerField label={label} layout={{type: 'vbox', align: 'left'}}>
        {
          selectList && 
          selectList.list &&
          selectList.list.map(item => 
            this.radioField(
              setName, 
              item, 
              selectList.selectedItemId,
              isEnable,
              onSelected
            )
          )
        }
      </ContainerField>
    );
  }

  private editGrid(items: EditItem[], nameTitle: string, valueTitle) {
    const { onItemEdited, edit, onApplyFilters } = this.props;
    const { isEnableClear, isEnableSave, isEnableApplyFilters } = edit;

    const store = Ext.create('Ext.data.Store', {
        data: items && items.slice(),
        listeners: {
          update: onItemEdited && 
                  ((store, record, operation, modifiedFieldNames, details) => {
                    if (modifiedFieldNames[0] === 'name') {
                      record.reject();
                    } else {
                      //HACK: Need for displaying new value.
                      record.set('valueCount', 1);

                      onItemEdited(record.data);
                    }
                  })
        }
    });
   
    return (
      <Grid 
        store={store} 
        flex={1} 
        shadow 
        height={450}
        columnLines={true}
        // plugins={[
        //   { type: 'cellediting', triggerEvent: 'singletap' },
        //   'selectionreplicator'
        // ]}
        plugins={['cellediting', 'selectionreplicator']}
        selectable={{
          rows: true,
          cells: true,
          columns: true,
          drag: true,
          extensible: 'y'
        }}
      >
        <Toolbar docked="top">
            <Button 
              text="Apply filters" 
              flex={1} 
              disabled={!isEnableApplyFilters}
              handler={onApplyFilters}
            />
        </Toolbar>
      
        <Column text={nameTitle} dataIndex="name" flex={1} extensible={false} />
        <Column 
          text={valueTitle} 
          dataIndex="value" 
          flex={1} 
          editable={true}
          renderer={
            (value, { data }: { data: EditItem }) => 
              data.valueCount == 1 ? value : `(${data.valueCount} values)` 
          }
        />

        <Toolbar docked="bottom">
            <Button 
              text="Clear" 
              flex={1} 
              disabled={!isEnableClear}
              handler={() => this.showClearDialog()}
            />
            <Button 
              text="Save" 
              flex={1} 
              disabled={!isEnableSave}
              handler={() => this.showSaveDialog()}
            />
        </Toolbar>
      </Grid>
    );
  }   

  private showSaveDialog() {
    const { onEditItemsSaving } = this.props;

    Ext.Msg.confirm(
      'Saving changes', 
      'Do you want to save the changes?',
      (buttonId: string) => onEditItemsSaving && onEditItemsSaving()
    );
  }

  private showClearDialog() {
    const { onEditItemsCleared } = this.props;

    Ext.Msg.confirm(
      'Clearing changes', 
      'Do you want to clear the changes??',
      (buttonId: string) => onEditItemsCleared && onEditItemsCleared()
    );
  }
}

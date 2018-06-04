import * as React from 'react';
import { Container, ComboBoxField, Panel, FormPanel, RadioField, ContainerField, Grid, Column, Toolbar, Button } from '@extjs/ext-react';
import { CostBlockInputState, EditItem, CheckItem } from '../States/CostBlock'
import { NamedId } from '../../Common/States/NamedId';
import { SelectList, MultiSelectList } from '../../Common/States/SelectList';
import { Filter } from './Filter';

Ext.require('Ext.grid.plugin.CellEditing');

export interface CostBlockActions {
  onCountrySelected(countryId: string)
  onCostElementSelected(costElementId: string)
  // onCostElementFilterSelectionChanged(
  //   costElementId: string, 
  //   filterItemId: string,
  //   isSelected: boolean)
  // onCostElementFilterReseted()
  onInputLevelSelected(inputLevelId: string)
  // onInputLevelFilterSelectionChanged(
  //   inputLevelId: string, 
  //   filterItemId: string,
  //   isSelected: boolean)
  // onInputLevelReseted()
}

export interface SelectListFilter {
  selectList: SelectList<NamedId>
  filter: CheckItem[],
  filterName: string,
  isVisibleFilter: boolean
}

export interface CostBlockProps {
  country: SelectList<NamedId>,
  costElement: SelectListFilter & {
    description: string
  }
  inputLevel: SelectListFilter
  editItems: EditItem[]
}

export class CostBlock extends React.Component<CostBlockProps & CostBlockActions> {
  public render() {
    const { country, costElement, inputLevel } = this.props;

    return (
      <Container layout={{ type: 'hbox', align: 'stretch '}}>
        <Container flex={1} layout="vbox" shadow>
          <Container layout="hbox">
            <FormPanel flex={1}>
              {this.countryCombobox(country)}
              {
                this.radioFieldSet(
                  'costelements', 
                  costElement.selectList, 
                  'Cost Elements', 
                  costElement => this.props.onCostElementSelected(costElement.id)
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
              />
            }
          </Container>

          <Panel title="Description" padding="10">
            {costElement.description}
          </Panel>
        </Container>

        <Container flex={1} layout="vbox" padding="0px 0px 0px 5px">
          <Container layout="hbox">
            <FormPanel flex={1}>
              {
                this.radioFieldSet(
                  'inputlevels', 
                  inputLevel.selectList, 
                  'Input Level',
                  inputLevel => this.props.onInputLevelSelected(inputLevel.id)
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
              />
            }
          </Container>

          {this.editGrid([], 'Name title', 'Value title')}
        </Container>
      </Container>
    );
  }

  private countryCombobox(country: SelectList<NamedId>) {
    const countryStore = Ext.create('Ext.data.Store', {
        data: country.list
    });

    const selectedCountry = 
        countryStore.getData()
                    .findBy(item => (item.data as NamedId).id === country.selectedItemId);

    return (
        <ComboBoxField 
            label="Select a Country:"
            //width="50%"
            displayField="name"
            valueField="id"
            queryMode="local"
            store={countryStore}
            selection={selectedCountry}
            onChange={(combobox, newValue, oldValue) => this.props.onCountrySelected(newValue)}
        />
    );
  }

  private radioField(
    name: string, 
    item: NamedId, 
    selectedCostElementId: string,
    onSelected: (item: NamedId) => void
  ) {
    return (
      <RadioField 
          key={item.id} 
          boxLabel={item.name} 
          name={name} 
          checked={item.id === selectedCostElementId}
          onCheck={radioField => onSelected(item)}
      />
    );
  }

  private radioFieldSet(
    setName: string, 
    selectList: SelectList<NamedId>, 
    label: string, 
    onSelected: (item: NamedId) => void
  ) {    
    return (
      <ContainerField label={label} layout={{type: 'vbox', align: 'left'}}>
        {
          selectList && 
          selectList.list.map(item => 
            this.radioField(
              setName, 
              item, 
              selectList.selectedItemId,
              onSelected
            )
          )
        }
      </ContainerField>
    );
  }

  private editGrid(items: EditItem[], nameTitle: string, valueTitle) {
    const store = Ext.create('Ext.data.Store', {
        //data: items
        data: [
          {id:'1', name:'test1', value: 10},
          {id:'2', name:'test2', value: 20},
          {id:'3', name:'test3', value: 30},
          {id:'4', name:'test4', value: 40},
          {id:'5', name:'test5', value: 50},
          {id:'6', name:'test7', value: 60},
        ]
    });

    return (
      <Grid 
        store={store} 
        flex={1} 
        shadow 
        height={400}
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
        <Column text={nameTitle} dataIndex="name" flex={1} extensible={false}/>
        <Column text={valueTitle} dataIndex="value" flex={1} editable={true}/>

        <Toolbar docked="bottom">
            <Button text="Clear" flex={1}/>
            <Button text="Save" flex={1}/>
        </Toolbar>
      </Grid>
    );
  }   
}

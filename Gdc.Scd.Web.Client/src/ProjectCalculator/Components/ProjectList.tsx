import * as React from "react";
import { Grid, Column, DateColumn, Toolbar, Button } from "@extjs/ext-react";
import { Store, Model } from "../../Common/States/ExtStates";
import { Project } from "../States/Project";
import { NamedId } from "../../Common/States/CommonStates";

const DataIndexes = {
    name: 'name',
    user: 'user',
    creationDate: 'creationDate'
}

export interface ProjectListActions {
    onAdd?(store: Store<Project>)
    onEdit?(store: Store<Project>, selectedProject: Project)
    onDelete?(store: Store<Project>, selectedProject: Project)
    onInit?()
    onSelectProject?(selectedProject: Project)
}

export interface ProjectListProps extends ProjectListActions {
    url: string
    selectedProject: Project
}

// export interface ProjectListState {
//     selectedItem: Model<Project>
// }

export class ProjectList extends React.PureComponent<ProjectListProps> {
    private store: Store<Project>
    private selectable = { mode: 'single' }

    constructor(prop: ProjectListProps) {
        super(prop)

        this.store = this.createStore(prop.url)
        // this.state = {
        //     selectedItem: null
        // }
    }

    public render() {
        return (
            <Grid 
                store={this.store} 
                selectable={this.selectable} 
                onSelect={this.onSelect} 
                onDeselect={this.onDeselect}
                masked={{ xtype: "loadmask" }}
                emptyText="No projects is available..."
                columnLines={true} 
                border={true}
                plugins={['pagingtoolbar']}
            >
                <DateColumn text="CreationDate" dataIndex={DataIndexes.creationDate} format="Y-m-d H:i:s" flex={1}/>
                <Column text="Name" dataIndex={DataIndexes.name} flex={1}/>
                <Column text="User" dataIndex={DataIndexes.name} renderer={this.userRenderer} flex={1}/>

                <Toolbar layout="hbox" docked="top">
                    <Button text="Add" handler={this.onAdd} flex={1}/>
                    <Button text="Edit" handler={this.onEdit} flex={1}/>
                    <Button text="Delete" handler={this.onDelete} flex={1}/>
                </Toolbar>  
            </Grid>
        )
    }

    public componentDidMount() {
        const { onInit } = this.props;

        onInit && onInit();
    }

    private createStore(url: string): Store<Project> {
        return Ext.create('Ext.data.Store', {
            fields: [ 
                { name: DataIndexes.name, type: 'string' },
                { name: DataIndexes.user },
                { name: DataIndexes.creationDate, type: 'date' },
            ],
            autoLoad: true,
            remoteSort: true,
            pageSize : 100,
            proxy: {
                type: 'ajax',
                url,
                reader: { 
                    type: 'json',
                    rootProperty: 'items',
                    totalProperty: 'total'
                }
            }
        });
    }

    private userRenderer(user: NamedId) {
        return user && user.name
    }

    // private onSelect = (grid, selecteItems: Model<Project>[]) => {
    //     this.setState({ selectedItem: selecteItems[0] })
    // }

    // private onDeselect = (grid, selecteItems: Model<Project>[]) => {
    //     this.setState({ selectedItem: null })
    // }

    private onSelect = (grid, selectedItems: Model<Project>[]) => {
        const { onSelectProject } = this.props;

        onSelectProject && onSelectProject(selectedItems[0].data)
    }

    private onAdd = () => {
        const { onAdd } = this.props;

        onAdd && onAdd(this.store);
    }

    private onEdit = () => {
        const { onEdit, selectedProject } = this.props;

        onEdit && onEdit(this.store, selectedProject)
    }

    private onDelete = () => {
        const { onDelete, selectedProject } = this.props;

        onDelete && onDelete(this.store, selectedProject)
    }
}
import * as React from "react";
import { FormPanel, TextField, Toolbar, Button } from "@extjs/ext-react";
import { Project, ProjectItem } from "../States/Project";
import { ProjectItemEditData } from "../States/ProjectCalculatorState";
import { ProjectItemsGrid } from "./ProjectItemsGrid";
import { Model, StoreOperation } from "../../Common/States/ExtStates";

export interface ProjectEditorActions {
    onInit?()
    onBackToList?(project: Project)
    onSave?(project: Project)
    onUpdateProjectItems?(records: Model<ProjectItem>[], operation: StoreOperation, dataIndex: string)
}

export interface ProjectEditorProps extends ProjectEditorActions {
    project: Project
    projectItemEditData: ProjectItemEditData    
}

export interface ProjectEditorState {
    saveButtonDisabled: boolean
}

export class ProjectEditor extends React.PureComponent<ProjectEditorProps, ProjectEditorState> {
    private projectItemGrid: ProjectItemsGrid
    private projectNameField

    constructor(props: ProjectEditorProps) {
        super(props);

        this.state = {
            saveButtonDisabled: true
        }
    }

    public componentDidMount() {
        const { onInit } = this.props;

        onInit && onInit();
    }

    public render() {
        const project = this.props.project || {} as Project;

        return (
            <FormPanel layout="vbox">
                {/* <DatePickerField label="Creation date" value={project.creationDate} readOnly={true}/> */}
                {/* <TextField label="User" value={project.user && project.user.name} readOnly={true}/> */}
                <TextField 
                    ref={this.setProjectNameFieldRef} 
                    label="Project name" 
                    value={project.name} 
                    required
                    onChange={this.onProjectChanged}
                />
                
                <ProjectItemsGrid 
                    ref={this.setProjectItemsGrid} 
                    projectItemEditData={this.props.projectItemEditData}
                    projectItems={project.projectItems || []}
                    onAddRecord={this.setSaveButtonDisabled}
                    onRemoveRecord={this.setSaveButtonDisabled}
                    onUpdateRecordSet={this.onUpdateProjectItems}
                />

                <Toolbar layout="hbox" docked="bottom">
                    <Button text="Back To List" handler={this.onBackToList} flex={1}/>
                    <Button text="Save" handler={this.onSave} flex={1} disabled={this.state.saveButtonDisabled}/>
                </Toolbar>  
            </FormPanel>
        )
    }

    private setProjectItemsGrid = grid => {
        this.projectItemGrid = grid;
    }

    private setProjectNameFieldRef = field => {
        this.projectNameField = field;
    }

    private setSaveButtonDisabled = () => {
        this.setState({ 
            saveButtonDisabled: !this.isValidProject() 
        })
    }

    private onProjectChanged = () => {
        this.setSaveButtonDisabled();
    }

    private onUpdateProjectItems = (
        records: Model<ProjectItem>[], 
        operation: StoreOperation, 
        dataIndex: string
    ) => {
        this.setSaveButtonDisabled();

        const { onUpdateProjectItems } = this.props;

        onUpdateProjectItems && onUpdateProjectItems(records, operation, dataIndex);
    }

    private onBackToList = (project: Project) => {
        const { onBackToList } = this.props;

        onBackToList && onBackToList(project);
    }

    private onSave = () => {
        Ext.Msg.confirm(
            'Save Project', 
            'Do you want to save the project?',
            (buttonId: string) => {
                if (buttonId == 'yes') {
                    const { onSave } = this.props;

                    onSave && onSave(this.getEditedProject());
                }
            },
            this
        );
    }

    private getEditedProject = () => {
        return {
            ...this.props.project,
            name: this.projectNameField.getValue(),
            projectItems: this.projectItemGrid.getEditedProjectItems()
        } as Project
    }

    private isValidProject = () => {
        const project = this.getEditedProject();

        return (
            project.name &&
            project.projectItems &&
            project.projectItems.every(projectItem => (
                projectItem.wgId &&
                projectItem.countryId &&
                projectItem.availability &&
                projectItem.availability.start &&
                projectItem.availability.start.day != null &&
                projectItem.availability.start.hour != null &&
                projectItem.availability.end &&
                projectItem.availability.end.day != null &&
                projectItem.availability.end.hour != null &&
                projectItem.reactionTypeId &&
                projectItem.serviceLocationId &&
                projectItem.duration &&
                projectItem.duration.value != null &&
                projectItem.duration.periodType != null
            ))
        );
    }
}
import { get } from "./Ajax";

export class DomainService<T> {

    private controllerName: string;

    public constructor(cname: string) {
        this.controllerName = cname;
    }

    public getAll(): Promise<T[]> {
        return get<T[]>(this.controllerName, 'GetAll');
    }

}
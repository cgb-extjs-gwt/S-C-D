import { DomainService } from "./DomainService";

export class CacheDomainService<T> {

    private static all: any = {}; // promises cache, singleton

    private controllerName: string;

    public constructor(cname: string) {
        this.controllerName = cname;
    }

    public getAll(): Promise<T[]> {
        let p = CacheDomainService.all[this.controllerName];
        if (!p) {
            p = new DomainService<T>(this.controllerName).getAll();
            CacheDomainService.all[this.controllerName] = p;
        }
        return p;
    }
}
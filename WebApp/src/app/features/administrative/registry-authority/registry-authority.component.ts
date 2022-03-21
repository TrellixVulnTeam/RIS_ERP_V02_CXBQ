import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { SelectionModel } from '@angular/cdk/collections';
import { RegistryAuthorityService } from './registry-authority.service';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../../../service/CommonMessage/notification.service';

@Component({
    selector: 'app-registry-authority',
    templateUrl: './registry-authority.component.html',
    styleUrls: ['./registry-authority.component.css'],
})
export class RegistryAuthorityComponent implements OnInit {

    rowData: any;
    dataSaved = false;
    registryAuthorityForm: any;
    allRegistryAuthority: Observable<any[]>;
    selection = new SelectionModel<any>(true, []);
    massage = null;
    displayedColumns: string[] = ['RegistryAuthorityName', 'Remarks'];
    isRegistryAuthorityEdit: boolean = false;
    rowSelected: boolean = false;

    selectedCountry: any;
    allCountry: any[];

    selectedRegistryAuthority: any;
    registryAuthoritys: any[];


    // for delete data modal
    display: boolean = false;
    showDialog() {
        if (this.rowData == null) {
            return this.notifyService.ShowNotification(3, 'Please select row');
        }
        else
            this.display = true;
    }


    // for Insert and update data modal
    displayBasic: boolean = false;
    showBasicDialog() {
        this.registryAuthorityForm.reset();
        this.massage = null;
        this.dataSaved = false;
        this.displayBasic = true;
    }

    onRowSelect(event) {
        this.rowSelected = true;
        this.rowData = event.data;
    }
    onRowUnselect(event) {
        this.rowSelected = false;
        this.rowData = null;
    }



    constructor(private formbulider: FormBuilder, private RegistryAuthorityService: RegistryAuthorityService, private toastr: ToastrService, private notifyService: NotificationService) {

    }

    ngOnInit() {

        this.formInit();
        this.loadAllCountryCboList();
        this.loadAllRegistryAuthoritys();
    }

    formInit() {
        this.registryAuthorityForm = this.formbulider.group({
            country_id: [null, [Validators.required]],
            registry_authority_name: [null, [Validators.required]],
            registry_authority_short_name: [null, [Validators.required]],
            remarks: ['']

        });
    }


    loadAllCountryCboList() {
        this.RegistryAuthorityService.getAllCountryCboList().subscribe(data => {
            this.allCountry = data;
        });
    }

    loadAllRegistryAuthoritys() {
        this.RegistryAuthorityService.getAllRegistryAuthority().subscribe(data => {
            this.registryAuthoritys = data;
        });
    }

    loadRegistryAuthorityToEdit() {
        if (this.rowData == null) {
            return this.notifyService.ShowNotification(3, 'Please select row');
        }
        let registry_authority_id = this.rowData.registry_authority_id;
        this.RegistryAuthorityService.GetRegistryAuthorityById(registry_authority_id).subscribe(data => {
            this.massage = null;
            this.dataSaved = false;
            this.registryAuthorityForm.controls['country_id'].setValue(data.country_id);
            this.registryAuthorityForm.controls['registry_authority_name'].setValue(data.registry_authority_name);
            this.registryAuthorityForm.controls['registry_authority_short_name'].setValue(data.registry_authority_short_name);
            this.registryAuthorityForm.controls['remarks'].setValue(data.remarks);
            this.isRegistryAuthorityEdit = true;
        });
        this.displayBasic = true;
    }

    deleteRegistryAuthorityInfo() {
        if (this.rowData == null) {
            return this.notifyService.ShowNotification(3, 'Please select row');
        }
        let registry_authority_id = this.rowData.registry_authority_id;
        this.RegistryAuthorityService.DeleteRegistryAuthority(registry_authority_id).subscribe(data => {
            if (data.MessageType == 2) {
                this.registryAuthoritys.splice(this.registryAuthoritys.findIndex(item => item.registry_authority_id === registry_authority_id), 1);
            }
            this.notifyService.ShowNotification(data.MessageType, data.CurrentMessage)
        });
        this.display = false;
    }

    onFormSubmit() {
        const RegistryAuthoritydata = this.registryAuthorityForm.value;
        if (!(RegistryAuthoritydata.registry_authority_name)) {
            return this.notifyService.ShowNotification(2, "Please input Registry Authority Name")
        }
        if (!(RegistryAuthoritydata.country_id)) {
            return this.notifyService.ShowNotification(2, "Please select country")
        }
        if (this.isRegistryAuthorityEdit) {
            RegistryAuthoritydata.registry_authority_id = this.rowData.registry_authority_id;
            this.RegistryAuthorityService.UpdateRegistryAuthority(RegistryAuthoritydata).subscribe(result => {
                debugger
                this.notifyService.ShowNotification(result.MessageType, result.CurrentMessage);
                if (result.MessageType == 1) {
                    this.clear();
                    this.registryAuthoritys.splice(this.registryAuthoritys.findIndex(item => item.registry_authority_id === RegistryAuthoritydata.registry_authority_id), 1);
                    this.registryAuthoritys.unshift(result.Data[0]);
                    this.selectedRegistryAuthority = result.Data[0];
                    this.rowSelected = true;
                    this.rowData = result.Data[0];
                    this.displayBasic = false;
                }
            });
        }
        else {
            this.RegistryAuthorityService.CreateRegistryAuthority(JSON.stringify(RegistryAuthoritydata)).subscribe(result => {
                debugger
                this.notifyService.ShowNotification(result.MessageType, result.CurrentMessage);
                if (result.MessageType == 1) {
                    this.clear();
                    this.registryAuthoritys.unshift(result.Data[0]);
                    this.selectedRegistryAuthority = result.Data[0];
                    this.rowSelected = true;
                    this.rowData = result.Data[0];
                    this.displayBasic = false;
                }
            });
        }
    }

    resetForm() {
        this.registryAuthorityForm.reset();
        this.isRegistryAuthorityEdit = false;
        this.formInit();
    }

    clear() {
        this.resetForm();
    }
}


import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FloatLabelModule } from 'primeng/floatlabel';

@Component({
  selector: 'app-job-input',
  standalone: true,
  imports: [
    FormsModule, 
    CardModule, 
    ButtonModule, 
    InputTextModule,
    FloatLabelModule
  ],
  template: `
    <p-card header="Try me!" styleClass="mb-3">
      <div class="p-fluid">
        <p-floatlabel variant="on">
          <input 
            pInputText 
            id="job_input" 
            [ngModel]="inputTextValue"
            (ngModelChange)="onValueChange($event)"
            [disabled]="disabled"
            autocomplete="off" 
            class="w-full" />
          <label for="job_input">Input Text to Process</label>
        </p-floatlabel>

        <p-button
          [label]="submitLabel"
          icon="pi pi-play"
          [disabled]="!canSubmit"
          [loading]="loading"
          (onClick)="startJob.emit()"
          styleClass="mt-3">
        </p-button>
      </div>
    </p-card>
  `
})
export class JobInputComponent {
  @Input() inputTextValue: string = '';
  @Input() disabled: boolean = false;
  @Input() loading: boolean = false;
  @Input() canSubmit: boolean = false;
  @Input() submitLabel: string = '';
  
  @Output() inputTextValueChange = new EventEmitter<string>();
  @Output() startJob = new EventEmitter<void>();

  onValueChange(value: string): void {
    this.inputTextValue = value;
    this.inputTextValueChange.emit(value); 
  }
}
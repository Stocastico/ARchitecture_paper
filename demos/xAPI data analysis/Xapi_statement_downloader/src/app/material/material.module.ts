import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {MatButtonModule, MatButtonToggleModule, MatFormFieldModule,MatInputModule, MatSelectModule } from '@angular/material';
//import {MatInputModule} from '@angular/material/input';
import { FormsModule } from '@angular/forms';

const materialComponents = [ MatButtonModule, MatButtonToggleModule, MatInputModule, MatSelectModule, FormsModule];

@NgModule({
  exports: [materialComponents],
  imports: [materialComponents]
})
export class MaterialModule { }

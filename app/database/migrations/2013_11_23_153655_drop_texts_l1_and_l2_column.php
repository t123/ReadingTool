<?php

use Illuminate\Database\Migrations\Migration;

class DropTextsL1AndL2Column extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::table('texts', function($table) {
            $table->dropColumn('l1Text', 'l2Text');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        //
    }

}

<?php

use Illuminate\Database\Migrations\Migration;

class AddLengthColumnToTexts extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::table('terms', function($table) {
            $table->smallInteger('length');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::table('terms', function($table) {
            $table->dropColumn('length');
        });
    }

}

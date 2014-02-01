<?php

use Illuminate\Database\Migrations\Migration;

class AddArchiveToLanguages extends Migration {

    public function up() {
        Schema::table('languages', function($table) {
            $table->boolean('archived')->default('0');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::table('languages', function($table) {
            $table->dropColumn('archived');
        });
    }

}

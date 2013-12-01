<?php

use Illuminate\Database\Migrations\Migration;

class AddGroupIdToTerms extends Migration {

    public function up() {
        Schema::table('terms', function($table) {
            $table->integer('group_id')->unsigned()->nullable();
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::table('terms', function($table) {
            $table->dropColumn('group_id');
        });
    }

}
